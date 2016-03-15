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
    public class ProtobufDatomSerializer2 : IDatomSerializer
    {
        #region Private variables and constructor

        private TypeRegistry _typeRegistry;

        private static class MessageTypeHandlers<T>
        {
            public static MessageTypeHandler<T> Value;
        }
        private Dictionary<ushort, object> _messageTypeHandlerByTypeRegistryTypeId;

        private delegate Dictionary<ulong, MessageBuilder> GetMessageBuildersByType(IGrouping<ushort, Datom> datoms);
        private Dictionary<ushort, GetMessageBuildersByType> _getMessageBuildersForTypeByTypeRegistryTypeId;

        private delegate void WireUpReferencesForTypeDelegate(Dictionary<ushort, Dictionary<ulong, MessageBuilder>> types);
        private Dictionary<ushort, WireUpReferencesForTypeDelegate> _wireUpReferencesForTypeByTypeRegistryTypeId;

        private Dictionary<ushort, Func<object, object>> _getMessageObjectFromMessageBuilderForType;

        private Dictionary<Type, PrimitiveSerializer> _primitiveSerializers { get; }

        public ProtobufDatomSerializer2(
                TypeRegistry typeRegistry,
                params Assembly[] assemblies
            )
        {
            _typeRegistry = typeRegistry;
            _primitiveSerializers = GetPrimitiveSerializers();
            _messageTypeHandlerByTypeRegistryTypeId = new Dictionary<ushort, object>();
            _getMessageBuildersForTypeByTypeRegistryTypeId = new Dictionary<ushort, GetMessageBuildersByType>();

            var protobufMessageTypes = assemblies
                .SelectMany(assembly => assembly.ExportedTypes.Where(type => typeof(IMessage).IsAssignableFrom(type)))
                .ToArray();

            var errors = protobufMessageTypes.Where(x => !_typeRegistry.IdByType.ContainsKey(x)).Select(x => x.FullName);
            if (errors.Any())
            {
                throw new InvalidOperationException($"IMessages [{string.Join(", ", errors)}] are not registered types. "
                      + "Make sure your registerTypePredicate matches them and you include the assemblies in which "
                      + "they are declared when you construct your TypeRegistry.");
            }
            
            
            for (var i = 0; i < protobufMessageTypes.Length; i++)
            {
                var messageType = protobufMessageTypes[i];
                var typeRegistryTypeId = _typeRegistry.IdByType[messageType];
                var buildMessageTypeHandler = typeof(ProtobufDatomSerializer2)
                    .GetMethod($"{nameof(ProtobufDatomSerializer2.BuildMessageTypeHandler)}")
                    .MakeGenericMethod(messageType);
                var messageTypeHandler = buildMessageTypeHandler.Invoke(this, new object[] { typeRegistryTypeId });

                var MessageTypeIdFieldName = $"{nameof(MessageTypeHandlers<bool>.Value)}";
                SetValueOnGenericStaticClass(typeof(MessageTypeHandlers<>), messageType, MessageTypeIdFieldName, messageTypeHandler);

                _messageTypeHandlerByTypeRegistryTypeId.Add(typeRegistryTypeId, messageTypeHandler);

                _getMessageBuildersForTypeByTypeRegistryTypeId.Add(typeRegistryTypeId, GetGetMessageBuildersByTypeDelegate(messageType));

                _wireUpReferencesForTypeByTypeRegistryTypeId.Add(typeRegistryTypeId, GetWireUpReferencesForTypeAction(messageType, typeRegistryTypeId));
            }
        }

        private WireUpReferencesForTypeDelegate GetWireUpReferencesForTypeAction(Type messageType, ushort typeRegistryTypeId)
        {
            var reflectedMethod = typeof(ProtobufDatomSerializer2)
                    .GetMethod($"{nameof(ProtobufDatomSerializer2.WireUpReferencesForType)}")
                    .MakeGenericMethod(messageType);
            var typesParameter = Expression.Parameter(typeof(Dictionary<ushort, Dictionary<ulong, MessageBuilder>>), "types");
            var callExpression = Expression.Call(
                    Expression.Constant(this),
                    reflectedMethod,
                    Expression.Constant(typeRegistryTypeId),
                    typesParameter
                );
            return Expression.Lambda(typeof(WireUpReferencesForTypeDelegate), callExpression, typesParameter).Compile() as WireUpReferencesForTypeDelegate;
        }

        private GetMessageBuildersByType GetGetMessageBuildersByTypeDelegate(Type messageType)
        {
            var reflectedMethod = typeof(ProtobufDatomSerializer2)
                    .GetMethod($"{nameof(ProtobufDatomSerializer2.GetMessageBuildersForType)}")
                    .MakeGenericMethod(messageType);
            var datomsParameter = Expression.Parameter(typeof(IGrouping<ushort, Datom>), "datoms");
            var callExpression = Expression.Call(
                    Expression.Constant(this),
                    reflectedMethod,
                    datomsParameter
                );
            var callAndCastToObject = Expression.Convert(callExpression, typeof(Dictionary<ulong, MessageBuilder>));

            return Expression.Lambda(
                typeof(GetMessageBuildersByType),
                callAndCastToObject,
                datomsParameter
            ).Compile() as GetMessageBuildersByType;
        }

        #endregion

        #region Public Interface 
        public IEnumerable<T> DeserializeMany<T>(IEnumerable<Datom> datoms) 
        {
            if (datoms.Any())
            {
                var types = GetMessageBuildersByTypeAndIdentity(datoms);

                var returnTypeId = MessageTypeHandlers<T>.Value.TypeId;
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

        #endregion

        #region Deserialization functions

        private Dictionary<ushort, Dictionary<ulong, MessageBuilder>> GetMessageBuildersByTypeAndIdentity(IEnumerable<Datom> datoms)
        {
            var types = datoms.GroupBy(x => x.Type)
                    .Select(type => 
                        new KeyValuePair<ushort, Dictionary<ulong, MessageBuilder>>(
                            type.Key,
                            _getMessageBuildersForTypeByTypeRegistryTypeId[type.Key](type)
                        )
                    ).ToDictionary(x => x.Key, x => x.Value);

            foreach (var typeId in types.Keys)
            {
                _wireUpReferencesForTypeByTypeRegistryTypeId[typeId](types);
            }

            return types;
        }

        
        private void WireUpReferencesForType<T> (ushort typeId, Dictionary<ushort, Dictionary<ulong, MessageBuilder>> types)
        {
            MessageTypeHandler<T> messageTypeHandler = (MessageTypeHandler<T>)_messageTypeHandlerByTypeRegistryTypeId[typeId];

            var instances = types[typeId];
            foreach (var instanceKeyValue in instances)
            {
                foreach (var reference in instanceKeyValue.Value.References)
                {
                    if (types.ContainsKey(reference.ReferencedTypeId))
                    {
                        var referencedType = types[reference.ReferencedTypeId];
                        if (referencedType.ContainsKey(reference.ReferencedIdentity))
                        {
                            var field = messageTypeHandler.FieldsByParameterNumber[reference.Parameter] as ReferenceField<T>;
                            var message = (T)instanceKeyValue.Value.Message;
                            field.SetReference(message, (int)reference.RepeatedFieldIndex, referencedType[reference.ReferencedIdentity].Message);
                        }
                    }
                }
            }
        }

        private Dictionary<ulong, MessageBuilder> GetMessageBuildersForType<T>(IGrouping<ushort, Datom> datoms)
        {
            MessageTypeHandler<T> messageTypeHandler = (MessageTypeHandler<T>)_messageTypeHandlerByTypeRegistryTypeId[datoms.Key];

            return datoms.GroupBy(x => x.Identity)
                    .Select(groupedByIdentity => 
                        new KeyValuePair<ulong, MessageBuilder>(
                            groupedByIdentity.Key,
                            GetMessageBuilder(messageTypeHandler, groupedByIdentity)
                        )
                    ).ToDictionary(x => x.Key, x => x.Value);
        }

        private MessageBuilder GetMessageBuilder<T> (MessageTypeHandler<T> messageTypeHandler, IGrouping<ulong, Datom> datoms)
        {
            var messageBuilder = new MessageBuilder();
            messageBuilder.Message = messageTypeHandler.Factory();

            datoms.GroupBy(y => new ParameterWithArrayIndex()
            {
                Parameter = y.Parameter,
                ParameterArrayIndex = y.ParameterArrayIndex
            })
            .Select(grouping => grouping.OrderByDescending(x => x.TransactionId).First())
            .ToList()
            .ForEach(x => {
                var fieldHandler = messageTypeHandler.FieldsByParameterNumber[x.Parameter];
                if (fieldHandler.GetFieldClass() == FieldClass.Simple)
                {
                    var simpleFieldHandler = (SimpleField<T>)fieldHandler;
                    simpleFieldHandler.Deserialize((T)messageBuilder.Message, x);
                }
                else
                {
                    var referenceFieldHandler = (ReferenceField<T>)fieldHandler;
                    messageBuilder.References.Add(new ReferenceFromBuilder() {
                        Parameter = x.Parameter,
                        ReferencedTypeId = referenceFieldHandler.ReferencedTypeId,
                        ReferencedIdentity = referenceFieldHandler.GetReferencedIdentity(x),
                        RepeatedFieldIndex = x.ParameterArrayIndex,
                    }); 
                }
            });

            return messageBuilder;
        }

        #endregion

        #region building MessageTypeHandlers with Reflection and Expression trees

        private MessageTypeHandler<T> BuildMessageTypeHandler<T>(ushort typeRegistryTypeId)
        {
            Type messageType = typeof(T);

            var descriptorProperty = messageType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(x => x.Name == "Descriptor");

            var descriptor = (MessageDescriptor)descriptorProperty.GetValue(null);
            var fields = descriptor.Fields.InFieldNumberOrder();

            var fieldByFieldNumber = fields
                .Select((field) => new KeyValuePair<int, FieldDescriptor>(field.FieldNumber, field))
                .ToDictionary(x => x.Key, x => x.Value);


            var constructInstance = Expression.New(messageType.GetConstructors().First());

            return new MessageTypeHandler<T>()
            {
                TypeId = typeRegistryTypeId,
                Factory = Expression.Lambda<Func<T>>(constructInstance).Compile(),
                FieldsByParameterNumber = fields.Select(FieldHandlerBuilder(messageType)).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private Func<FieldDescriptor, int, KeyValuePair<ushort, FieldHandler>> FieldHandlerBuilder(Type messageType)
        {
            return (discriptor, fieldIndex) =>
            {
                var titleCaseName = $"{discriptor.Name.Substring(0, 1).ToUpper()}{discriptor.Name.Substring(1)}";
                var property = messageType.GetProperty(titleCaseName);

                Type propertyOuterType = null;
                Type propertyType;
                var isRepeated = property.PropertyType.GetGenericTypeDefinition() == typeof(RepeatedField<>);
                if (isRepeated)
                {
                    propertyOuterType = property.PropertyType;
                    propertyType = propertyOuterType.GetGenericArguments()[0];
                }
                else
                {
                    propertyType = property.PropertyType;
                }
                var isReference = typeof(IMessage).IsAssignableFrom(propertyType);
                var isPrimitive = _primitiveSerializers.ContainsKey(propertyType);

                if (!isReference && !isPrimitive)
                {
                    throw new InvalidOperationException($"Unable to find a primitive serializer for type {property.PropertyType.FullName}.");
                }

                var definition = new FieldHandlerDefinition()
                {
                    Property = property,
                    MessageType = messageType,
                    PropertyOuterType = propertyOuterType,
                    PropertyType = propertyType,
                    IsReference = isReference,
                    IsRepeated = isRepeated,
                    PrimitiveSerializer = isPrimitive ? _primitiveSerializers[propertyType] : null
                };

                FieldHandler toReturn;
                if (isReference)
                {
                    var buildBuildReferenceField = typeof(ProtobufDatomSerializer2)
                        .GetMethod($"{nameof(ProtobufDatomSerializer2.BuildReferenceField)}")
                        .MakeGenericMethod(messageType);

                    toReturn = buildBuildReferenceField.Invoke(this, new object[] { definition }) as FieldHandler;
                }
                else
                {
                    var buildBuildSimpleField = typeof(ProtobufDatomSerializer2)
                        .GetMethod($"{nameof(ProtobufDatomSerializer2.BuildSimpleField)}")
                        .MakeGenericMethod(messageType);

                    toReturn = buildBuildSimpleField.Invoke(this, new object[] { definition }) as FieldHandler;
                }
                return new KeyValuePair<ushort, FieldHandler>((ushort)discriptor.FieldNumber, toReturn);
            };
        }

        private ReferenceField<T> BuildReferenceField<T>(FieldHandlerDefinition definition) {
            var toReturn = new ReferenceField<T>();
            var messageTypeParameter = Expression.Parameter(definition.MessageType, "message");
            var repeatedFieldIndexParameter = Expression.Parameter(typeof(int), "repeatedFieldIndex");
            var referencedObjectParameter = Expression.Parameter(typeof(object), "referencedObject");
            var referencedObjectCasted = Expression.Convert(referencedObjectParameter, definition.PropertyType);
            if (definition.IsRepeated)
            {
                var insertIntoRepeatedFieldMethod = definition.PropertyOuterType.GetMethod($"{nameof(RepeatedField<bool>.Insert)}");
                var insertExpression = Expression.Call(
                        messageTypeParameter,
                        insertIntoRepeatedFieldMethod,
                        repeatedFieldIndexParameter,
                        referencedObjectCasted
                    );
                var insertFunction = Expression.Lambda<Action<T, int, object>>(
                        insertExpression, 
                        messageTypeParameter,
                        repeatedFieldIndexParameter,
                        referencedObjectParameter
                    ).Compile();
                toReturn.SetReference = insertFunction;
            }
            else
            {
                var fieldExpression = Expression.PropertyOrField(messageTypeParameter, definition.Property.Name);
                var setFieldExpression = Expression.Assign(fieldExpression, referencedObjectCasted);
                var setFieldFunction = Expression.Lambda<Action<T, int, object>>(
                        setFieldExpression,
                        messageTypeParameter,
                        repeatedFieldIndexParameter,
                        referencedObjectParameter
                    ).Compile();
                toReturn.SetReference = setFieldFunction;
            }

            //var datomParameter = Expression.Parameter(typeof(Datom), "datom");
            //var datomValueAccess = Expression.Property(datomParameter, $"{nameof(Datom.Value)}");
            //var convertToULongMethod = typeof(BitConverter).GetMethod($"{nameof(BitConverter.ToUInt64)}");
            //var getReferencedId = Expression.Call(convertToULongMethod, datomValueAccess, Expression.Constant(0));
            //toReturn.GetReferencedIdentity = Expression.Lambda<Func<Datom, ulong>>(getReferencedId, datomParameter).Compile();
            toReturn.GetReferencedIdentity = (datom) => BitConverter.ToUInt64(datom.Value, 0);
            toReturn.ReferencedTypeId = _typeRegistry.IdByType[definition.PropertyType];

            return toReturn;
        }

        private SimpleField<T> BuildSimpleField<T>(FieldHandlerDefinition definition)
        {
            var toReturn = new SimpleField<T>();
            var messageParameter = Expression.Parameter(definition.MessageType, "message");
            var datomParameter = Expression.Parameter(typeof(Datom), "datom");
            var datomValue = Expression.Property(datomParameter, $"{nameof(Datom.Value)}");
            var deserializedValue = definition.PrimitiveSerializer.GetDeserializeExpression(datomValue);
            if (definition.IsRepeated)
            {
                var datomArrayIndex = Expression.Property(datomParameter, $"{nameof(Datom.ParameterArrayIndex)}");
                var insertIntoRepeatedFieldMethod = definition.PropertyOuterType.GetMethod($"{nameof(RepeatedField<bool>.Insert)}");
                var insertExpression = Expression.Call(
                        messageParameter,
                        insertIntoRepeatedFieldMethod,
                        datomArrayIndex,
                        deserializedValue
                    );
                var insertFunction = Expression.Lambda<Action<T, Datom>>(
                        insertExpression,
                        messageParameter,
                        datomParameter
                    ).Compile();
                toReturn.Deserialize = insertFunction;
            }
            else
            {
                var fieldExpression = Expression.PropertyOrField(messageParameter, definition.Property.Name);
                var setFieldExpression = Expression.Assign(fieldExpression, datomParameter);
                var setFieldFunction = Expression.Lambda<Action<T, Datom>>(
                        setFieldExpression,
                        messageParameter,
                        datomParameter
                    ).Compile();
                toReturn.Deserialize = setFieldFunction;
            }

            return toReturn;
        }
        #endregion

        #region Helper functions
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
            if (type == typeof(string))
            {
                return string.Empty;
            }
            return Activator.CreateInstance(type);
        }

        private void SetValueOnGenericStaticClass(Type genericType, Type genericArgument, string fieldName, object value)
        {
            genericType.MakeGenericType(genericArgument).GetField(fieldName).SetValue(null, value);
        }

        //public void LogException(Exception ex)
        //{
        //    System.Console.WriteLine(ex.Message + "\r\n" + ex.StackTrace);
        //}
        #endregion

        #region Private classes

        private class PrimitiveSerializer
        {
            public Func<Expression, Expression> GetSerializeExpression;
            public Func<Expression, Expression> GetDeserializeExpression;
        }

        private class MessageTypeHandler<T>
        {
            public ushort TypeId;
            public Dictionary<ushort, FieldHandler> FieldsByParameterNumber;
            public Func<T> Factory;
        }

        private enum FieldClass
        {
            Simple = 0,
            Repeated = 1
        }

        private class FieldHandlerDefinition
        {
            public PropertyInfo Property;
            public Type MessageType;
            public Type PropertyOuterType;
            public Type PropertyType;
            public PrimitiveSerializer PrimitiveSerializer;
            public bool IsReference;
            public bool IsRepeated;
        }

        private class FieldHandler
        {
            public virtual FieldClass GetFieldClass() => FieldClass.Simple;
        }

        private class SimpleField<T> : FieldHandler
        {
            public override FieldClass GetFieldClass() => FieldClass.Simple;
            public Action<T, Datom> Deserialize;
            public Func<T, IEnumerable<Datom>> Serialize;
        }

        private class ReferenceField<T> : FieldHandler
        {
            public override FieldClass GetFieldClass() => FieldClass.Repeated;
            public Func<T, IEnumerable<Datom>> Serialize;
            public ushort ReferencedTypeId;
            public Func<Datom, ulong> GetReferencedIdentity;
            public Action<T, int, object> SetReference;
        }

        private class MessageBuilder
        {
            public ulong Identity;
            public object Message;
            public List<ReferenceFromBuilder> References = new List<ReferenceFromBuilder>();
        }

        private struct ReferenceFromBuilder
        {
            public ushort ReferencedTypeId;
            public ulong ReferencedIdentity;
            public ushort Parameter;
            public uint RepeatedFieldIndex;
        }

        private struct Reference
        {
            public uint Type;
            public ulong Identity;
        }

        private struct ParameterWithArrayIndex
        {
            public ushort Parameter;
            public uint ParameterArrayIndex;
        }
        #endregion
    }
}
