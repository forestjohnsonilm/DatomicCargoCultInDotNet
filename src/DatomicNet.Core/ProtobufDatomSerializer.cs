using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using Google.Protobuf.Collections;
using System.Linq.Expressions;

namespace DatomicNet.Core
{
    public class ProtobufDatomSerializer : IDatomSerializer
    {
        public ProtobufDatomSerializer(
                TypeRegistry typeRegistry,
                params Assembly[] assemblies
            )
        {
            _typeRegistry = typeRegistry;
            _primitiveSerializers = GetPrimitiveSerializers();
            _serializers = new Dictionary<Type, MessageSerializer<object>>();
            _singleMessageSerializers = new Dictionary<Type, SingleMessageSerializer>();

            var protobufMessageTypes = assemblies
                .SelectMany(assembly => assembly.ExportedTypes.Where(type => typeof(IMessage).IsAssignableFrom(type)));

            var errors = protobufMessageTypes.Where(x => !_typeRegistry.IdByType.ContainsKey(x)).Select(x => x.FullName);
            if (errors.Any())
            {
                throw new InvalidOperationException($"IMessages [{string.Join(", ", errors)}] are not registered types. "
                      + "Make sure your registerTypePredicate matches them and you include the assemblies in which "
                      + "they are declared when you construct your TypeRegistry.");
            }

            foreach (var messageType in protobufMessageTypes)
            {
                var typeId = _typeRegistry.IdByType[messageType];

                //var singleMessageSerializerType = typeof(SingleMessageSerializer<>).MakeGenericType(messageType);
                var serializer = new SingleMessageSerializer() {
                    Deserialize = BuildSingleMessageDeserialzer(messageType, typeId)
                };


                _singleMessageSerializers.Add(messageType, serializer);

            }
        }

        private readonly TypeRegistry _typeRegistry;

        private Dictionary<Type, PrimitiveSerializer> _primitiveSerializers { get; }
        private Dictionary<Type, MessageSerializer<object>> _serializers { get; }
        private Dictionary<Type, SingleMessageSerializer> _singleMessageSerializers { get; }
        //// is this faster than Dictionary<Type, MessageSerializer>
        //private static class MessageSerializer<T>
        //{
        //    public static Func<IEnumerable<T>, IEnumerable<Datom>> Serialize = null;
        //    public static Func<IEnumerable<Datom>, IEnumerable<T>> Deserialize = null;
        //    public static Func<IEnumerable<Datom>, T> SingleMessageDeserializer = null;
        //    public static Func<T, IEnumerable<Datom>> SingleMessageSerializer = null;
        //}

        public IEnumerable<T> DeserializeMany<T>(IEnumerable<Datom> datoms) 
        {
            if (datoms.Any())
            {
                var types = datoms.GroupBy(x => x.Type)
                    .Select(type =>
                        new KeyValuePair<uint, Dictionary<ulong, SingleMessage>>(
                            type.First().Type,
                            type.GroupBy(x => x.Identity)
                                .Select(instance =>
                                    new KeyValuePair<ulong, SingleMessage>(
                                        instance.First().Identity,
                                        _singleMessageSerializers[_typeRegistry.TypesById[instance.First().Type]].Deserialize(instance)
                                    )
                                ).ToDictionary(x => x.Key, x => x.Value)
                        )
                    ).ToDictionary(x => x.Key, x => x.Value);

                foreach (var typeKeyValue in types)
                {
                    foreach (var instanceKeyValue in typeKeyValue.Value)
                    {
                        foreach (var reference in instanceKeyValue.Value.References)
                        {
                            if (types.ContainsKey(reference.ReferencedType))
                            {
                                var referencedType = types[reference.ReferencedType];
                                if (referencedType.ContainsKey(reference.ReferencedIdentity))
                                {
                                    reference.Setter((IMessage)instanceKeyValue.Value.Message, (object)referencedType[reference.ReferencedIdentity].Message);
                                }
                            }
                        }
                    }
                }

                var returnTypeId = _typeRegistry.IdByType[typeof(T)];
                if (types.ContainsKey(returnTypeId))
                {
                    return types[returnTypeId].Select(x => (T)x.Value.Message);
                }
                return Enumerable.Empty<T>();
            }
            else
            {
                return Enumerable.Empty<T>();
            }
        }

        public T Deserialize<T>(IEnumerable<Datom> datoms)
        {
            return DeserializeMany<T>(datoms).FirstOrDefault();
        }

        public IEnumerable<Datom> Serialize<T>(IEnumerable<T> objects)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Datom> Serialize<T>(T @object)
        {
            throw new NotImplementedException();
        }

        private Func<IEnumerable<Datom>, SingleMessage> BuildSingleMessageDeserialzer(Type messageType, uint typeId)
        {
            var descriptorProperty = messageType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(x => x.Name == "Descriptor");

            var descriptor = (MessageDescriptor)descriptorProperty.GetValue(null);

            var orderedDatomsParameter = Expression.Parameter(typeof(Datom[]), "orderedDatoms");
            var fields = descriptor.Fields.InFieldNumberOrder();
            var datomValueMember = typeof(Datom).GetProperty($"{nameof(Datom.Value)}");

            var fieldIndexByFieldNumber = fields
                .Select((field, fieldIndex) => new KeyValuePair<int, int>(field.FieldNumber, fieldIndex))
                .ToDictionary(x => x.Key, x => x.Value);

            var memberMaps = fields.Select((field, fieldIndex) => {
                var titleCaseName = $"{field.Name.Substring(0, 1).ToUpper()}{field.Name.Substring(1)}";
                var property = messageType.GetProperty(titleCaseName);
                
                Expression getValue;
                if(_primitiveSerializers.ContainsKey(property.PropertyType))
                {
                    var getDatom = Expression.ArrayAccess(orderedDatomsParameter, Expression.Constant(fieldIndex));
                    var getValueBytes = Expression.MakeMemberAccess(getDatom, datomValueMember);

                    getValue = Expression.Condition(
                        Expression.NotEqual(getDatom, Expression.Constant(null)),
                        _primitiveSerializers[property.PropertyType].GetDeserializeExpression(getValueBytes),
                        Expression.Constant(CreateInstance(property.PropertyType))
                    );
                }
                else if(typeof(IMessage).IsAssignableFrom(property.PropertyType))
                {
                    return null;
                }
                else if (property.PropertyType.GetGenericTypeDefinition() == typeof(RepeatedField<>))
                {
                    return null;
                }
                else
                {
                    throw new InvalidOperationException($"Unable to find a primitive serializer for type {property.PropertyType.FullName}.");
                }

                return Expression.Bind(property, getValue);
            }).Where(x => x != null).ToArray();

            var constructInstance = Expression.New(messageType.GetConstructors().First());
            var createAndAssign = Expression.MemberInit(constructInstance, memberMaps);
            var createAndAssignAsIMessage = Expression.Convert(createAndAssign, typeof(IMessage));

            var fieldsCount = fields.Count();

            //var exception = Expression.Parameter(typeof(Exception));
            //var logExceptionMethod = typeof(ProtobufDatomSerializer).GetMethod("LogException");
            //var logException = Expression.Call(Expression.Constant(this), logExceptionMethod, exception);
            //var logError = Expression.Catch(exception, Expression.Block(logException, Expression.Convert(constructInstance, typeof(IMessage))));
            //var tryCatchExpression = Expression.TryCatch(createAndAssignAsIMessage, logError);
            //var lambdaExpression = Expression.Lambda<Func<Datom[], IMessage>>(tryCatchExpression, orderedDatomsParameter);

            var lambdaExpression = Expression.Lambda<Func<Datom[], IMessage>>(createAndAssignAsIMessage, orderedDatomsParameter);

            var compiledExpression = lambdaExpression.Compile();

            var idGetter = fields.First();
            var referenceGetterByFieldIndex = fields.Select(field =>
            {
                var titleCaseName = $"{field.Name.Substring(0, 1).ToUpper()}{field.Name.Substring(1)}";
                var property = messageType.GetProperty(titleCaseName);
                if (typeof(IMessage).IsAssignableFrom(property.PropertyType))
                {
                    var referencedTypeId = _typeRegistry.IdByType[property.PropertyType];
                    var messageParam = Expression.Parameter(typeof(IMessage), "message");
                    var messageParamAsType = Expression.Convert(messageParam, messageType);
                    var valueProperty = Expression.Property(messageParamAsType, property);
                    var valueParam = Expression.Parameter(typeof(object), "referencedValue");
                    var valueParamAsType = Expression.Convert(valueParam, property.PropertyType);
                    var setValue = Expression.Assign(valueProperty, valueParamAsType);
                    var setValueLambda = Expression.Lambda<Action<IMessage, object>>(setValue, messageParam, valueParam);
                    var setValueCompiled = setValueLambda.Compile();

                    Func<Datom, SingleMessage.Reference> refGetter = (datom) => {
                        if (datom != null)
                        {
                            return new SingleMessage.Reference()
                            {
                                ReferencedType = referencedTypeId,
                                ReferencedIdentity = BitConverter.ToUInt64(datom.Value, 0),
                                Setter = setValueCompiled
                            };
                        }
                        return null;
                    };
                    return refGetter;
                }
                return null;
            })
            .ToArray();

            return (datoms) => {
                var orderedDatoms = new Datom[fieldsCount];

                foreach(var datom in datoms)
                {
                    var fieldIndex = fieldIndexByFieldNumber[datom.Parameter];
                    var preExisting = orderedDatoms[fieldIndex];
                    if (preExisting == null || preExisting.TransactionId < datom.TransactionId)
                    {
                        orderedDatoms[fieldIndex] = datom;
                    }
                }

                return new SingleMessage()
                {
                    Message = compiledExpression(orderedDatoms),
                    References = orderedDatoms.Select((datom, id) => referenceGetterByFieldIndex[id] != null ? referenceGetterByFieldIndex[id](datom) : null).Where(x => x != null).ToList()
                };
            };
        }

        private class PrimitiveSerializer
        {
            public Func<Expression, Expression> GetSerializeExpression;
            public Func<Expression, Expression> GetDeserializeExpression;
        }

        private class MessageSerializer<T>
        {
            public Func<IEnumerable<T>, IEnumerable<Datom>> Serialize = null;
            public Func<IEnumerable<Datom>, IEnumerable<T>> Deserialize = null;
        }

        private class SingleMessageSerializer
        {
            public Func<IEnumerable<Datom>, SingleMessage> Deserialize = null;
            public Func<IMessage, IEnumerable<Datom>> Serialize = null;
        }

        private class SingleMessage
        {
            public IMessage Message { get; set; }
            public List<Reference> References { get; set; }

            public class Reference
            {
                public uint ReferencedType;
                public ulong ReferencedIdentity;
                public Action<IMessage, object> Setter;
            }
        }

        private Dictionary<Type, PrimitiveSerializer> GetPrimitiveSerializers()
        {
            return new List<KeyValuePair<Type, PrimitiveSerializer>>()
            {
                GetBitConverterSerializer(typeof(bool)),
                GetBitConverterSerializer(typeof(short)),
                GetBitConverterSerializer(typeof(int)),
                GetBitConverterSerializer(typeof(long)),
                GetBitConverterSerializer(typeof(ushort)),
                GetBitConverterSerializer(typeof(uint)),
                GetBitConverterSerializer(typeof(ulong)),
                GetBitConverterSerializer(typeof(float)),
                GetBitConverterSerializer(typeof(double)),
                new KeyValuePair<Type, PrimitiveSerializer> (
                    typeof(string),
                    new PrimitiveSerializer() {
                        GetDeserializeExpression = (param) => Expression.Call(
                            Expression.Constant(Encoding.UTF8),
                            typeof(Encoding).GetMethods()
                            .First(x => x.Name == $"{nameof(Encoding.GetString)}" && x.GetParameters().Count() == 1 && x.GetParameters()[0].ParameterType == typeof(byte[])),
                            param
                        ),
                        GetSerializeExpression = (param) => Expression.Call(
                            Expression.Constant(Encoding.UTF8),
                            typeof(Encoding).GetMethods()
                            .First(x => x.Name == $"{nameof(Encoding.GetBytes)}" && x.GetParameters().Count() == 1 && x.GetParameters()[0].ParameterType == typeof(string)),
                            param
                        )
                    }
                ),
                new KeyValuePair<Type, PrimitiveSerializer> (
                    typeof(Guid),
                    new PrimitiveSerializer() {
                        GetDeserializeExpression = (param) => Expression.New(
                            typeof(Guid)
                                .GetConstructors(BindingFlags.Public)
                                .First(x => x.GetParameters().Count() == 1 && x.GetParameters()[0].ParameterType == typeof(byte[])),
                            param
                        ),
                        GetSerializeExpression = (param) => Expression.Call(
                            param,
                            typeof(Guid).GetMethod($"{nameof(Guid.ToByteArray)}")
                        )
                    }
                )
            }.ToDictionary(x => x.Key, x => x.Value);
        }

        private KeyValuePair<Type, PrimitiveSerializer> GetBitConverterSerializer(Type type)
        {
            return new KeyValuePair<Type, PrimitiveSerializer>(
                type,
                new PrimitiveSerializer()
                {
                    GetDeserializeExpression = (param) => {
                        return Expression.Call(
                            typeof(BitConverter)
                            .GetMethods()
                            .First(x => x.GetParameters().Count() == 2 && x.ReturnType == type),
                            param, Expression.Constant(0)
                        );
                    },
                    GetSerializeExpression = (param) => {
                        return Expression.Call(
                            typeof(BitConverter)
                            .GetMethods()
                            .First(x => x.Name == $"{nameof(BitConverter.GetBytes)}" && x.GetParameters()[0].ParameterType == type),
                            param
                        );
                    },
                }
            );
        }

        private object CreateInstance(Type type)
        {
            if(type == typeof(string))
            {
                return string.Empty;
            }
            return Activator.CreateInstance(type);
        }

        public void LogException(Exception ex)
        {
            System.Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
        }
    }

}
