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
        #region Private variables and constructor

        private TypeRegistry _typeRegistry;

        private static class MessageTypeHandlers<T>
        {
            public static MessageTypeHandler<T> Value;
        }
        private Dictionary<ushort, object> _messageTypeHandlerByTypeRegistryTypeId;

        private delegate Dictionary<ulong, MessageBuilder> GetMessageBuildersByTypeDelegate(IGrouping<ushort, Datom> datoms);
        private Dictionary<ushort, GetMessageBuildersByTypeDelegate> _getMessageBuildersForTypeByTypeRegistryTypeId;

        private delegate void WireUpReferencesForTypeDelegate(Dictionary<ushort, Dictionary<ulong, MessageBuilder>> types);
        private Dictionary<ushort, WireUpReferencesForTypeDelegate> _wireUpReferencesForTypeByTypeRegistryTypeId;

        private Dictionary<Type, PrimitiveSerializer> _primitiveSerializers { get; }

        private BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        public ProtobufDatomSerializer(
                TypeRegistry typeRegistry,
                params Assembly[] assemblies
            )
        {
            _typeRegistry = typeRegistry;
            _primitiveSerializers = GetPrimitiveSerializers();
            _messageTypeHandlerByTypeRegistryTypeId = new Dictionary<ushort, object>();
            _getMessageBuildersForTypeByTypeRegistryTypeId = new Dictionary<ushort, GetMessageBuildersByTypeDelegate>();
            _wireUpReferencesForTypeByTypeRegistryTypeId = new Dictionary<ushort, WireUpReferencesForTypeDelegate>();

            var protobufMessageTypes = assemblies
                .SelectMany(assembly => assembly.ExportedTypes.Where(type => typeof(IMessage).IsAssignableFrom(type)))
                .ToArray();

            var errors = protobufMessageTypes.Where(x => !_typeRegistry.IsTypeRegistered(x))
                .Select(x => x.FullName);
            if (errors.Any())
            {
                throw new InvalidOperationException($"IMessages [{string.Join(", ", errors)}] are not registered types. "
                      + "Make sure your registerTypePredicate matches them and you include the assemblies in which "
                      + "they are declared when you construct your TypeRegistry.");
            }
            
            
            for (var i = 0; i < protobufMessageTypes.Length; i++)
            {
                var messageType = protobufMessageTypes[i];
                var typeRegistration = _typeRegistry.TypeRegistrationByType(messageType);
                var buildMessageTypeHandler = typeof(ProtobufDatomSerializer)
                    .GetMethod($"{nameof(ProtobufDatomSerializer.BuildMessageTypeHandler)}", PrivateInstance)
                    .MakeGenericMethod(messageType);
                var messageTypeHandler = buildMessageTypeHandler.Invoke(this, new object[] { typeRegistration });

                var MessageTypeIdFieldName = $"{nameof(MessageTypeHandlers<bool>.Value)}";
                SetValueOnGenericStaticClass(typeof(MessageTypeHandlers<>), messageType, MessageTypeIdFieldName, messageTypeHandler);

                _messageTypeHandlerByTypeRegistryTypeId.Add(typeRegistration.TypeId, messageTypeHandler);

                _getMessageBuildersForTypeByTypeRegistryTypeId.Add(typeRegistration.TypeId, GetGetMessageBuildersByTypeDelegate(messageType));

                _wireUpReferencesForTypeByTypeRegistryTypeId.Add(typeRegistration.TypeId, GetWireUpReferencesForTypeAction(messageType, typeRegistration.TypeId));
            }
        }

        private WireUpReferencesForTypeDelegate GetWireUpReferencesForTypeAction(Type messageType, ushort typeRegistryTypeId)
        {
            var reflectedMethod = typeof(ProtobufDatomSerializer)
                    .GetMethod($"{nameof(ProtobufDatomSerializer.WireUpReferencesForType)}", PrivateInstance)
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

        private GetMessageBuildersByTypeDelegate GetGetMessageBuildersByTypeDelegate(Type messageType)
        {
            var reflectedMethod = typeof(ProtobufDatomSerializer)
                    .GetMethod($"{nameof(ProtobufDatomSerializer.GetMessageBuildersForType)}", PrivateInstance)
                    .MakeGenericMethod(messageType);
            var datomsParameter = Expression.Parameter(typeof(IGrouping<ushort, Datom>), "datoms");
            var callExpression = Expression.Call(
                    Expression.Constant(this),
                    reflectedMethod,
                    datomsParameter
                );
            var callAndCastToObject = Expression.Convert(callExpression, typeof(Dictionary<ulong, MessageBuilder>));

            return Expression.Lambda(
                typeof(GetMessageBuildersByTypeDelegate),
                callAndCastToObject,
                datomsParameter
            ).Compile() as GetMessageBuildersByTypeDelegate;
        }

        #endregion

        #region Public Interface 
        public IEnumerable<T> DeserializeMany<T>(IEnumerable<Datom> datoms) 
        {
            if (datoms.Any())
            {
                var types = GetMessageBuildersByTypeAndIdentity(datoms);

                var returnTypeId = MessageTypeHandlers<T>.Value.TypeRegistration.TypeId;
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
            return SerializeRecurse(@object, new SerializationSettings(), new HashSet<Reference>());
        }
        #endregion

        #region Serialization private methods

        private IEnumerable<Datom> SerializeRecurse<T>(T @object, SerializationSettings settings, HashSet<Reference> visited)
        {
            var typeHandler = MessageTypeHandlers<T>.Value;
            var typeId = typeHandler.TypeRegistration.TypeId;
            var identity = typeHandler.TypeRegistration.KeyGetter(@object);
            var reference = new Reference() { Type = typeId, Identity = identity };
            if (!visited.Contains(reference))
            {
                visited.Add(reference);

                var prototype = new Datom
                    (
                        settings.AggregateType,
                        settings.AggregateId,
                        typeId,
                        identity,
                        0,
                        0,
                        new byte[0],
                        1,
                        settings.Action
                    );

                typeHandler.FieldsByParameterNumber
                    .Where(x => x.Value.GetFieldClass() == FieldClass.Simple)
                    .SelectMany(x => ((SimpleField<T>)x.Value).Serialize(@object, prototype))
                    .Concat(
                        typeHandler.FieldsByParameterNumber
                        .Where(x => x.Value.GetFieldClass() == FieldClass.Reference)
                        .SelectMany(x => {
                            var referenceField = ((ReferenceField<T>)x.Value);
                            var serialized = referenceField.Serialize(@object, prototype);
                            //var referenced = referenceField.FollowReference();
                            // FollowReference() should return a TReference not an object -- refactor.
                            //if (referenced != null)
                        })
                    );
            }

        }

        #endregion

        #region Deserialization private methods

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
            messageBuilder.Message = messageTypeHandler.TypeRegistration.Factory(datoms.Key);

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

        private MessageTypeHandler<T> BuildMessageTypeHandler<T>(BaseTypeRegistration baseTypeRegistration)
        {
            Type messageType = typeof(T);
            var typeRegistration = (TypeRegistration<T>)baseTypeRegistration;

            var descriptorProperty = messageType.GetProperties(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(x => x.Name == "Descriptor");

            var descriptor = (MessageDescriptor)descriptorProperty.GetValue(null);
            var fields = descriptor.Fields.InFieldNumberOrder();

            var fieldByFieldNumber = fields
                .Select((field) => new KeyValuePair<int, FieldDescriptor>(field.FieldNumber, field))
                .ToDictionary(x => x.Key, x => x.Value);

            return new MessageTypeHandler<T>()
            {
                TypeRegistration = typeRegistration,
                FieldsByParameterNumber = fields.Select(FieldHandlerBuilder(typeRegistration)).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private Func<FieldDescriptor, int, KeyValuePair<ushort, FieldHandler>> FieldHandlerBuilder(BaseTypeRegistration typeRegistration)
        {
            return (discriptor, fieldIndex) =>
            {
                var messageType = typeRegistration.Type;
                var titleCaseName = $"{discriptor.Name.Substring(0, 1).ToUpper()}{discriptor.Name.Substring(1)}";
                var property = messageType.GetProperty(titleCaseName);

                Type propertyOuterType = null;
                Type propertyType;
                var isRepeated = property.PropertyType.GetTypeInfo().IsGenericType
                    ? property.PropertyType.GetGenericTypeDefinition() == typeof(RepeatedField<>) 
                    : false;

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
                    TypeRegistration = typeRegistration,
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
                    var buildBuildReferenceField = typeof(ProtobufDatomSerializer)
                        .GetMethod($"{nameof(ProtobufDatomSerializer.BuildReferenceField)}", PrivateInstance)
                        .MakeGenericMethod(messageType);

                    toReturn = buildBuildReferenceField.Invoke(this, new object[] { definition }) as FieldHandler;
                }
                else
                {
                    var buildBuildSimpleField = typeof(ProtobufDatomSerializer)
                        .GetMethod($"{nameof(ProtobufDatomSerializer.BuildSimpleField)}", PrivateInstance)
                        .MakeGenericMethod(messageType);

                    toReturn = buildBuildSimpleField.Invoke(this, new object[] { definition }) as FieldHandler;
                }
                return new KeyValuePair<ushort, FieldHandler>((ushort)discriptor.FieldNumber, toReturn);
            };
        }

        private ReferenceField<T> BuildReferenceField<T>(FieldHandlerDefinition definition) {
            var toReturn = new ReferenceField<T>();
            var messageTypeParameter = Expression.Parameter(definition.MessageType, "message");
            var fieldOnMessage = Expression.Property(messageTypeParameter, definition.Property);
            var repeatedFieldIndexParameter = Expression.Parameter(typeof(int), "repeatedFieldIndex");
            var referencedObjectParameter = Expression.Parameter(typeof(object), "referencedObject");
            var referencedObjectCasted = Expression.Convert(referencedObjectParameter, definition.PropertyType);
            if (definition.IsRepeated)
            {
                var insertIntoRepeatedFieldMethod = definition.PropertyOuterType.GetMethod($"{nameof(RepeatedField<bool>.Insert)}");
                var insertExpression = Expression.Call(
                        fieldOnMessage,
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
                var setFieldExpression = Expression.Assign(fieldOnMessage, referencedObjectCasted);
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
            if(!_typeRegistry.IsTypeRegistered(definition.PropertyType))
            {
                throw new InvalidOperationException($"Unable to create reference {definition.MessageType.FullName}.{definition.Property.Name}"
                    + $" because the type {definition.PropertyType.FullName} is not registered. ");
            }
            toReturn.ReferencedTypeId = _typeRegistry.TypeRegistrationByType(definition.PropertyType).TypeId;

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
                var setFieldExpression = Expression.Assign(fieldExpression, deserializedValue);
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
            public TypeRegistration<T> TypeRegistration;
            public Dictionary<ushort, FieldHandler> FieldsByParameterNumber;
        }

        private enum FieldClass
        {
            Simple = 0,
            Reference = 1
        }

        private class FieldHandlerDefinition
        {
            public BaseTypeRegistration TypeRegistration;
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
            public Func<T, Datom, IEnumerable<Datom>> Serialize;
        }

        private class ReferenceField<T> : FieldHandler
        {
            public override FieldClass GetFieldClass() => FieldClass.Reference;
            public Func<T, Datom, IEnumerable<Datom>> Serialize;
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

        private class SerializationSettings
        {
            public ushort AggregateType;
            public ulong AggregateId;
            public DatomAction Action = DatomAction.Assertion;
        }
        #endregion
    }
}
