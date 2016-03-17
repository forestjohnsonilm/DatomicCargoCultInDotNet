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

        private SchemaRegistry _typeRegistry;

        private static class MessageTypeHandlers<T>
        {
            public static MessageTypeHandler<T> Value;
        }
        private Dictionary<ushort, object> _messageTypeHandlerByTypeId;

        private Dictionary<Tuple<ushort, ushort>, Func<Datom, string>> _debugFormatters;

        private delegate Dictionary<ulong, MessageBuilder> GetMessageBuildersByTypeDelegate(IGrouping<ushort, Datom> datoms);
        private Dictionary<ushort, GetMessageBuildersByTypeDelegate> _getMessageBuildersForTypeByTypeId;

        private delegate void WireUpReferencesForTypeDelegate(Dictionary<ushort, Dictionary<ulong, MessageBuilder>> types);
        private Dictionary<ushort, WireUpReferencesForTypeDelegate> _wireUpReferencesForTypeByTypeId;

        private delegate IEnumerable<Datom> SerializeRecurseDelegate(object @object, SerializationSettings settings, HashSet<Reference> visited);
        private Dictionary<ushort, SerializeRecurseDelegate> _serializeRecurseForTypeByTypeId;

        private Dictionary<Type, PrimitiveSerializer> _primitiveSerializers { get; }

        private BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        public ProtobufDatomSerializer(
                SchemaRegistry typeRegistry,
                params Assembly[] assemblies
            )
        {
            _typeRegistry = typeRegistry;
            _primitiveSerializers = GetPrimitiveSerializers();
            _messageTypeHandlerByTypeId = new Dictionary<ushort, object>();
            _getMessageBuildersForTypeByTypeId = new Dictionary<ushort, GetMessageBuildersByTypeDelegate>();
            _wireUpReferencesForTypeByTypeId = new Dictionary<ushort, WireUpReferencesForTypeDelegate>();
            _serializeRecurseForTypeByTypeId = new Dictionary<ushort, SerializeRecurseDelegate>();
            _debugFormatters = new Dictionary<Tuple<ushort, ushort>, Func<Datom, string>>();

            var protobufMessageTypes = assemblies
                .SelectMany(assembly => assembly.ExportedTypes.Where(type => typeof(IMessage).IsAssignableFrom(type)))
                .ToArray();

            var errors = protobufMessageTypes.Where(x => !_typeRegistry.IsTypeRegistered(x))
                .Select(x => x.FullName);
            if (errors.Any())
            {
                throw new InvalidOperationException($"IMessages [{string.Join(", ", errors)}] are not registered types. "
                      + "Make sure your registerTypePredicate matches them and you include the assemblies in which "
                      + "they are declared when you construct your SchemaRegistry.");
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

                _messageTypeHandlerByTypeId.Add(typeRegistration.TypeId, messageTypeHandler);

                _getMessageBuildersForTypeByTypeId.Add(typeRegistration.TypeId, GetGetMessageBuildersByTypeDelegate(messageType));

                _wireUpReferencesForTypeByTypeId.Add(typeRegistration.TypeId, GetWireUpReferencesForTypeAction(messageType, typeRegistration.TypeId));

                _serializeRecurseForTypeByTypeId.Add(typeRegistration.TypeId, GetSerializeRecurseForType(messageType, typeRegistration.TypeId));
            }
        }


        #endregion

        public string DebugFormatDatom(Datom datom)
        {
            return _debugFormatters[new Tuple<ushort, ushort>(datom.Type, datom.Parameter)](datom);
        }



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
            var visited = new HashSet<Reference>();
            var settings = new SerializationSettings();

            return objects.SelectMany(x => SerializeRecurse(x, settings, visited));
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
            var identity = typeHandler.KeyGetter(@object);
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
                        settings.TransactionId,
                        settings.Action
                    );

                var simpleFieldsDatoms = typeHandler.FieldsByParameterNumber
                    .Where(x => x.Value.FieldClass == FieldClass.Simple)
                    .SelectMany(x => ((SimpleField<T>)x.Value).Serialize(@object, prototype));

                var referenceFieldsDatoms = typeHandler.FieldsByParameterNumber
                        .Where(x => x.Value.FieldClass == FieldClass.Reference)
                        .SelectMany(x =>
                        {
                            var referenceField = ((ReferenceField<T>)x.Value);
                            var recurse = _serializeRecurseForTypeByTypeId[referenceField.ReferencedTypeId];
                            return referenceField.Serialize(@object, prototype)
                                .Concat(
                                    referenceField.Follow(@object)
                                    .SelectMany(referenced => recurse(referenced, settings, visited))
                                );
                        });

                return simpleFieldsDatoms.Concat(referenceFieldsDatoms);
            } 
            else
            {
                return Enumerable.Empty<Datom>();
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
                            _getMessageBuildersForTypeByTypeId[type.Key](type)
                        )
                    ).ToDictionary(x => x.Key, x => x.Value);

            foreach (var typeId in types.Keys)
            {
                _wireUpReferencesForTypeByTypeId[typeId](types);
            }

            return types;
        }

        
        private void WireUpReferencesForType<T> (ushort typeId, Dictionary<ushort, Dictionary<ulong, MessageBuilder>> types)
        {
            MessageTypeHandler<T> messageTypeHandler = (MessageTypeHandler<T>)_messageTypeHandlerByTypeId[typeId];

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
            MessageTypeHandler<T> messageTypeHandler = (MessageTypeHandler<T>)_messageTypeHandlerByTypeId[datoms.Key];

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
            messageBuilder.Message = messageTypeHandler.Factory(datoms.Key);

            datoms.GroupBy(y => new ParameterWithArrayIndex()
            {
                Parameter = y.Parameter,
                ParameterArrayIndex = y.ParameterArrayIndex
            })
            .Select(grouping => grouping.OrderByDescending(x => x.TransactionId).First())
            .ToList()
            .ForEach(x => {
                var fieldHandler = messageTypeHandler.FieldsByParameterNumber[x.Parameter];
                if (fieldHandler.FieldClass == FieldClass.Simple)
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
                        ReferencedIdentity = referenceFieldHandler.DeserializeIdentity(x),
                        RepeatedFieldIndex = x.ParameterArrayIndex,
                    }); 
                }
            });

            return messageBuilder;
        }

        #endregion

        #region building MessageTypeHandlers 

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

            var idParameter = Expression.Parameter(typeof(ulong), "identity");
            var entityParameter = Expression.Parameter(messageType, "entity");
            var factory = Expression.Lambda<Func<ulong, T>>(typeRegistration.FactoryExpressionBuilder(idParameter), idParameter).Compile();
            var keyGetter = Expression.Lambda<Func<T, ulong>>(typeRegistration.KeyGetterExpressionBuilder(entityParameter), entityParameter).Compile();

            return new MessageTypeHandler<T>()
            {
                TypeRegistration = typeRegistration,
                Factory = factory,
                KeyGetter = keyGetter,
                FieldsByParameterNumber = fields
                    .Where(x => string.Compare(x.Name, baseTypeRegistration.KeyMember.Name, true) != 0)
                    .Select(FieldHandlerBuilder(typeRegistration))
                    .ToDictionary(x => x.Key, x => x.Value)
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
                    FieldDescriptor = discriptor,
                    MessageTypeRegistration = typeRegistration,
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
                        .MakeGenericMethod(messageType, propertyType);

                    toReturn = buildBuildReferenceField.Invoke(this, new object[] { definition }) as FieldHandler;
                }
                else
                {
                    var buildBuildSimpleField = typeof(ProtobufDatomSerializer)
                        .GetMethod($"{nameof(ProtobufDatomSerializer.BuildSimpleField)}", PrivateInstance)
                        .MakeGenericMethod(messageType, propertyType);

                    toReturn = buildBuildSimpleField.Invoke(this, new object[] { definition }) as FieldHandler;
                }

                Func<byte[], string> debugDeserializer;
                if(isPrimitive)
                {
                    var valueParameter = Expression.Parameter(typeof(byte[]), "value");
                    var deserialize = _primitiveSerializers[propertyType].GetDeserializeExpression(valueParameter);
                    var toStringMethod = propertyType.GetMethods().First(x => x.Name == "ToString");
                    var deserializeToString = Expression.Call(deserialize, toStringMethod);
                    debugDeserializer = Expression.Lambda<Func<byte[], string>>(deserializeToString, valueParameter).Compile();
                }
                else
                {
                    debugDeserializer = (value) => BitConverter.ToUInt64(value, 0).ToString();
                }

                _debugFormatters[new Tuple<ushort, ushort>(typeRegistration.TypeId, (ushort)discriptor.FieldNumber)] = (datom) => {
                    var valueFormat = isReference ? $"{propertyType.Name}[{{0}}]" : "{0}";
                    var fieldArrayIndex = isRepeated ? $"[{datom.ParameterArrayIndex}]": string.Empty;
                    return $"TX[{datom.TransactionId}] {datom.Action} {typeRegistration.Type.Name}[{datom.Identity}]"
                        + $".{discriptor.Name}{fieldArrayIndex} = {string.Format(valueFormat, debugDeserializer(datom.Value))}";
                };

                return new KeyValuePair<ushort, FieldHandler>((ushort)discriptor.FieldNumber, toReturn);
            };
        }

        private ReferenceField<TMessage> BuildReferenceField<TMessage, TProperty>(FieldHandlerDefinition definition) {
            var toReturn = new ReferenceField<TMessage>();
            var messageParameter = Expression.Parameter(definition.MessageType, "message");
            var fieldOnMessage = Expression.Property(messageParameter, definition.Property);
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
                var insertFunction = Expression.Lambda<Action<TMessage, int, object>>(
                        insertExpression, 
                        messageParameter,
                        repeatedFieldIndexParameter,
                        referencedObjectParameter
                    ).Compile();
                toReturn.SetReference = insertFunction;

                var fieldOnMessageAsIEnumerableObject = Expression.Convert(fieldOnMessage, typeof(IEnumerable<object>));
                toReturn.Follow = Expression.Lambda<Func<TMessage, IEnumerable<object>>>(fieldOnMessageAsIEnumerableObject, messageParameter).Compile();

                toReturn.Serialize = BuildSerializeFunction<TMessage, TProperty>(definition);
            }
            else
            {
                var setFieldExpression = Expression.Assign(fieldOnMessage, referencedObjectCasted);
                var setFieldFunction = Expression.Lambda<Action<TMessage, int, object>>(
                        setFieldExpression,
                        messageParameter,
                        repeatedFieldIndexParameter,
                        referencedObjectParameter
                    ).Compile();
                toReturn.SetReference = setFieldFunction;

                var yieldMethod = typeof(ProtobufDatomSerializer)
                    .GetMethod($"{nameof(ProtobufDatomSerializer.Yield)}", BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(typeof(object));

                var yieldReferencedObject = Expression.Call(yieldMethod, Expression.Convert(fieldOnMessage, typeof(object)));
                toReturn.Follow = Expression.Lambda<Func<TMessage, IEnumerable<object>>>(yieldReferencedObject, messageParameter).Compile();

                toReturn.Serialize = BuildSerializeFunction<TMessage, TProperty>(definition);
            }

            //var datomParameter = Expression.Parameter(typeof(Datom), "datom");
            //var datomValueAccess = Expression.Property(datomParameter, $"{nameof(Datom.Value)}");
            //var convertToULongMethod = typeof(BitConverter).GetMethod($"{nameof(BitConverter.ToUInt64)}");
            //var getReferencedId = Expression.Call(convertToULongMethod, datomValueAccess, Expression.Constant(0));
            //toReturn.DeserializeIdentity = Expression.Lambda<Func<Datom, ulong>>(getReferencedId, datomParameter).Compile();
            toReturn.DeserializeIdentity = (datom) => BitConverter.ToUInt64(datom.Value, 0);
            if(!_typeRegistry.IsTypeRegistered(definition.PropertyType))
            {
                throw new InvalidOperationException($"Unable to create reference {definition.MessageType.FullName}.{definition.Property.Name}"
                    + $" because the type {definition.PropertyType.FullName} is not registered. ");
            }
            toReturn.ReferencedTypeId = _typeRegistry.TypeRegistrationByType(definition.PropertyType).TypeId;

            return toReturn;
        }

        private SimpleField<TMessage> BuildSimpleField<TMessage, TProperty>(FieldHandlerDefinition definition)
        {
            var toReturn = new SimpleField<TMessage>();
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
                var insertFunction = Expression.Lambda<Action<TMessage, Datom>>(
                        insertExpression,
                        messageParameter,
                        datomParameter
                    ).Compile();
                toReturn.Deserialize = insertFunction;
                toReturn.Serialize = BuildSerializeFunction<TMessage, TProperty>(definition);
            }
            else
            {
                var fieldExpression = Expression.PropertyOrField(messageParameter, definition.Property.Name);
                var setFieldExpression = Expression.Assign(fieldExpression, deserializedValue);
                var setFieldFunction = Expression.Lambda<Action<TMessage, Datom>>(
                        setFieldExpression,
                        messageParameter,
                        datomParameter
                    ).Compile();
                toReturn.Deserialize = setFieldFunction;
                toReturn.Serialize = BuildSerializeFunction<TMessage, TProperty>(definition);
            }

            return toReturn;
        }

        private Func<TMessage, Datom, IEnumerable<Datom>> BuildSerializeFunction<TMessage, TProperty>(FieldHandlerDefinition definition)
        {
            var messageParameter = Expression.Parameter(definition.MessageType, "message");
            var prototypeParameter = Expression.Parameter(typeof(Datom), "prototype");

            var fieldOnMessage = Expression.Property(messageParameter, definition.Property);

            var messageTypeRegistrationType = typeof(TypeRegistration<>).MakeGenericType(definition.MessageType);

            var messageIdGetterExpressionBuilder = (Func<ParameterExpression, Expression>)messageTypeRegistrationType
                .GetField($"{nameof(TypeRegistration<int>.KeyGetterExpressionBuilder)}")
                .GetValue(definition.MessageTypeRegistration);

            var arrayElementParameter = Expression.Parameter(definition.PropertyType, "element");
            var arrayIndexParameter = Expression.Parameter(typeof(int), "i");
            var arrayIndexAsUInt = Expression.Convert(arrayIndexParameter, typeof(uint));

            var parameterIndexExpression = definition.IsRepeated ? (Expression)arrayIndexAsUInt : (Expression)Expression.Constant((uint)0, typeof(uint));
            var referencedObjectExpression = definition.IsRepeated ? (Expression)arrayElementParameter : (Expression)fieldOnMessage;
            Expression valueExpression;
            Expression shouldSerializeExpression;
            if(definition.IsReference)
            {
                var propertyTypeRegistration = (TypeRegistration<TProperty>)_typeRegistry.TypeRegistrationByType(definition.PropertyType);
                var arrayElementIdentityProperty = propertyTypeRegistration.KeyGetterExpressionBuilder(referencedObjectExpression);
                valueExpression = _primitiveSerializers[typeof(ulong)].GetSerializeExpression(arrayElementIdentityProperty);
                shouldSerializeExpression = Expression.NotEqual(referencedObjectExpression, Expression.Constant(null));
            } 
            else
            {
                valueExpression = definition.PrimitiveSerializer.GetSerializeExpression(referencedObjectExpression);
                shouldSerializeExpression = definition.PrimitiveSerializer.GetShouldSerializeExpression(referencedObjectExpression);
            }

            var messageIdentityExpression = messageIdGetterExpressionBuilder(messageParameter);

            var constructInstance = Expression.New(
                typeof(Datom).GetConstructors().OrderByDescending(x => x.GetParameters().Count()).First(),
                Expression.Property(prototypeParameter, $"{nameof(Datom.AggregateType)}"),                 //ushort aggregateTypeId,
                Expression.Property(prototypeParameter, $"{nameof(Datom.AggregateIdentity)}"),             //ulong aggregateId,
                Expression.Constant(definition.MessageTypeRegistration.TypeId, typeof(ushort)),            //ushort type,
                messageIdentityExpression,                                                                 //ulong identity,
                Expression.Constant((ushort)definition.FieldDescriptor.FieldNumber, typeof(ushort)),               //ushort parameter,
                parameterIndexExpression,                                                                  //uint parameterIndex,
                valueExpression,                                                                           //byte[] value,
                Expression.Property(prototypeParameter, $"{nameof(Datom.TransactionId)}"),                 //ulong transactionId,
                Expression.Property(prototypeParameter, $"{nameof(Datom.Action)}")                         //DatomAction action
            );

            Expression serializeExpression;
            if(definition.IsRepeated)
            {
                var mapperDelegateType = typeof(Func<,,>).MakeGenericType(definition.PropertyType, typeof(int), typeof(Datom));
                var mapper = Expression.Lambda(mapperDelegateType, constructInstance, arrayElementParameter, arrayIndexParameter);

                var iEnumerableType = typeof(IEnumerable<>).MakeGenericType(definition.PropertyType);
                var fieldOnMessageAsIEnumerable = Expression.Convert(fieldOnMessage, iEnumerableType);

                //public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate);
                var whereMethod = typeof(Enumerable).GetMethods()
                    .First(x => x.Name == $"{nameof(Enumerable.Where)}" && x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
                    .MakeGenericMethod(definition.PropertyType);

                //public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector);
                var selectMethod = typeof(Enumerable).GetMethods()
                    .First(x => x.Name == $"{nameof(Enumerable.Select)}" && x.GetParameters()[1].ParameterType.GetGenericArguments().Length == 3)
                    .MakeGenericMethod(definition.PropertyType, typeof(Datom));

                var shouldSerializeLambda = Expression.Lambda<Func<TProperty, bool>>(shouldSerializeExpression, arrayElementParameter);

                var whereExpression = Expression.Call(whereMethod, fieldOnMessageAsIEnumerable, shouldSerializeLambda);

                serializeExpression = Expression.Call(selectMethod, whereExpression, mapper);
            }
            else
            {
                var yieldMethod = typeof(ProtobufDatomSerializer)
                .GetMethod($"{nameof(ProtobufDatomSerializer.Yield)}", BindingFlags.Static | BindingFlags.NonPublic)
                .MakeGenericMethod(typeof(Datom));

                serializeExpression = Expression.Condition(
                    shouldSerializeExpression,
                    Expression.Call(yieldMethod, constructInstance),
                    Expression.Constant(Enumerable.Empty<Datom>(), typeof(IEnumerable<Datom>))
                );
            }

            return Expression.Lambda<Func<TMessage, Datom, IEnumerable<Datom>>>(
                serializeExpression,
                messageParameter,
                prototypeParameter
            ).Compile();
        }

        #endregion

        #region Helper functions

        private SerializeRecurseDelegate GetSerializeRecurseForType(Type messageType, ushort typeRegistryTypeId)
        {
            var reflectedMethod = typeof(ProtobufDatomSerializer)
                    .GetMethod($"{nameof(ProtobufDatomSerializer.SerializeRecurse)}", PrivateInstance)
                    .MakeGenericMethod(messageType);

            var objectParameter = Expression.Parameter(typeof(object), "@object");
            var castedObjectParameter = Expression.Convert(objectParameter, messageType);
            var settingsParameter = Expression.Parameter(typeof(SerializationSettings), "settings");
            var visitedParameter = Expression.Parameter(typeof(HashSet<Reference>), "visited");

            var callExpression = Expression.Call(
                    Expression.Constant(this),
                    reflectedMethod,
                    castedObjectParameter,
                    settingsParameter,
                    visitedParameter
                );
            return Expression.Lambda(
                typeof(SerializeRecurseDelegate),
                callExpression,
                objectParameter,
                settingsParameter,
                visitedParameter
            ).Compile() as SerializeRecurseDelegate;
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
                        GetShouldSerializeExpression = (param) => Expression.Not(Expression.Call(
                          typeof(string).GetMethod($"{nameof(string.IsNullOrWhiteSpace)}", BindingFlags.Static | BindingFlags.Public),
                          param
                        )),
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
                        GetShouldSerializeExpression = (param) => Expression.NotEqual(param, Expression.Constant(null)),
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
                    GetShouldSerializeExpression = (param) => Expression.NotEqual(param, Expression.Constant(Activator.CreateInstance(type))),
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

        private static IEnumerable<T> Yield<T>(T item)
        {
            yield return item;
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
            public Func<Expression, Expression> GetShouldSerializeExpression;
            public Func<Expression, Expression> GetSerializeExpression;
            public Func<Expression, Expression> GetDeserializeExpression;
        }

        private class MessageTypeHandler<T>
        {
            public TypeRegistration<T> TypeRegistration;
            public Func<T, ulong> KeyGetter;
            public Func<ulong, T> Factory;
            public Dictionary<ushort, FieldHandler> FieldsByParameterNumber;
        }

        private enum FieldClass
        {
            Simple = 0,
            Reference = 1
        }

        private class FieldHandlerDefinition
        {
            public FieldDescriptor FieldDescriptor;
            public BaseTypeRegistration MessageTypeRegistration;
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
            public virtual FieldClass FieldClass => FieldClass.Simple;
        }

        private class SimpleField<T> : FieldHandler
        {
            public override FieldClass FieldClass => FieldClass.Simple;
            public Action<T, Datom> Deserialize;
            public Func<T, Datom, IEnumerable<Datom>> Serialize;
        }

        private class ReferenceField<T> : FieldHandler
        {
            public override FieldClass FieldClass => FieldClass.Reference;
            public Func<T, Datom, IEnumerable<Datom>> Serialize;
            public Func<T, IEnumerable<object>> Follow;
            public ushort ReferencedTypeId;
            public Func<Datom, ulong> DeserializeIdentity;
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
            public ulong TransactionId;
            public DatomAction Action = DatomAction.Assertion;
        }
        #endregion
    }
}
