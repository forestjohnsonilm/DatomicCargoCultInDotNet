using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DatomicNet.Core
{
    public class TypeRegistry
    {
        private readonly IReadOnlyDictionary<ushort, Func<BaseTypeRegistration>> _typeRegistrationById;
        private readonly IReadOnlyDictionary<Type, Func<BaseTypeRegistration>> _typeRegistrationByType;
        private readonly IReadOnlyDictionary<ushort, Func<BaseTypeRegistration>> _typeRegistrationByAggregateId;

        private BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        public TypeRegistry(
                Func<Type, bool> registerTypePredicate,
                params Assembly[] assemblies
            )
        {
            var registeredTypes = assemblies
                .SelectMany(assembly => assembly.ExportedTypes.Where(type => registerTypePredicate(type)));

            var schemaConfigTypes = assemblies
                .SelectMany(assembly => assembly.ExportedTypes.Where(type => typeof(ISchemaConfiguration).IsAssignableFrom(type)));

            var errorSchemaChangeTypes = schemaConfigTypes.Where(x => !TypeCanBeActivated(x));

            if (errorSchemaChangeTypes.Any())
            {
                throw new InvalidOperationException($"{nameof(ISchemaConfiguration)}s {string.Join(", ", errorSchemaChangeTypes.Select(x => x.Name))}"
                                                  + $" are invalid because they do not have a default constructor.");
            }

            var schemaChanges = schemaConfigTypes.Select(x => (ISchemaConfiguration)Activator.CreateInstance(x));
            var typeRegistrations = schemaChanges.SelectMany(x => x.RegisterTypes);

            foreach (var registration in typeRegistrations)
            {
                SetTypeRegistrationForType(registration);
            }

            var errors = typeRegistrations.GroupBy(x => x.TypeId)
                .Where(x => x.Count() > 1)
                .Select(x => $"Type Id `{x.Key}` is associated with multiple types: [{string.Join(", ", x.Select(y => y.Type.FullName))}]");
            if (errors.Any())
            {
                throw new InvalidOperationException($"Invalid Schema: {string.Join(", ", errors)}.");
            }
            _typeRegistrationById = typeRegistrations.ToDictionary(x => x.TypeId, x => GetRegistrationGetterForType(x.Type));
            


            errors = typeRegistrations.GroupBy(x => x.Type)
                .Where(x => x.Count() > 1)
                .Select(x => $"Type `{x.Key.FullName}` is associated with multiple ids: [{string.Join(", ", x.Select(y => y.TypeId))}]");
            if (errors.Any())
            {
                throw new InvalidOperationException($"Invalid Schema: {string.Join(", ", errors)}.");
            }
            _typeRegistrationByType = typeRegistrations.ToDictionary(x => x.Type, x => GetRegistrationGetterForType(x.Type));

            errors = typeRegistrations.Where(x => x.AggregateId.HasValue).GroupBy(x => x.AggregateId)
                .Where(x => x.Count() > 1)
                .Select(x => $"Aggregate Type Id `{x.Key}` is associated with multiple types: [{string.Join(", ", x.Select(y => y.Type.FullName))}]");
            if (errors.Any())
            {
                throw new InvalidOperationException($"Invalid Schema: {string.Join(", ", errors)}.");
            }
            _typeRegistrationByAggregateId = typeRegistrations
                .Where(x => x.AggregateId.HasValue)
                .ToDictionary(x => x.AggregateId.Value, x => GetRegistrationGetterForType(x.Type));

            errors = typeRegistrations.Where(x => x.AggregateId.HasValue).GroupBy(x => x.Type)
                .Where(x => x.Count() > 1)
                .Select(x => $"Type `{x.Key}` is associated with multiple aggregate type ids: [{string.Join(", ", x.Select(y => y.AggregateId.Value))}]");
            if (errors.Any())
            {
                throw new InvalidOperationException($"Invalid Schema: {string.Join(", ", errors)}.");
            }

            var registeredButNotGivenId = registeredTypes.Where(x => GetRegistrationGetterForType(x)() == null).Select(x => x.FullName);
            if (registeredButNotGivenId.Any())
            {
                throw new InvalidOperationException(
                    $"Registered types [{string.Join(", ", registeredButNotGivenId)}] don't have Type Ids. "
                  + $"Create an {nameof(ISchemaConfiguration)} with a default RegisterTypes that includes them."
                );
            }
            var givenIdButNotRegistered = typeRegistrations.Where(x => !registeredTypes.Contains(x.Type)).Select(x => x.Type.FullName);
            if (givenIdButNotRegistered.Any())
            {
                throw new InvalidOperationException(
                    $"Types [{string.Join(", ", givenIdButNotRegistered)}] were mentioned in an {nameof(ISchemaConfiguration)} but not registered. "
                  + $"make sure your registerTypePredicate matches them and you include the assemblies in which they are declared."
                );
            }
        }

        public BaseTypeRegistration TypeRegistrationById (ushort id)
        {
            return _typeRegistrationById[id]();
        }

        public bool IsTypeRegistered(Type type)
        {
            return _typeRegistrationByType.ContainsKey(type);
        }

        public BaseTypeRegistration TypeRegistrationByType(Type type)
        {
            return _typeRegistrationByType[type]();
        }

        public BaseTypeRegistration TypeRegistrationByAggregateId (ushort id)
        {
            return _typeRegistrationByAggregateId[id]();
        }

        public TypeRegistration<T> TypeRegistrationGeneric<T>()
        {
            return ByType<T>.Value;
        }

        private Func<BaseTypeRegistration> GetRegistrationGetterForType(Type type)
        {
            var reflectedMethod = typeof(TypeRegistry)
                .GetMethod($"{nameof(TypeRegistry.TypeRegistrationGeneric)}")
                .MakeGenericMethod(type);
            var callExpression = Expression.Call(
                    Expression.Constant(this),
                    reflectedMethod
                );
            return Expression.Lambda<Func<BaseTypeRegistration>>(callExpression).Compile();
        }

        private void SetTypeRegistrationForType(BaseTypeRegistration registration)
        {
            var reflectedMethod = typeof(TypeRegistry)
                .GetMethod($"{nameof(TypeRegistry.SetTypeRegistrationForTypeGeneric)}", PrivateInstance)
                .MakeGenericMethod(registration.Type);

            reflectedMethod.Invoke(this, new object[] { registration });
        }

        private void SetTypeRegistrationForTypeGeneric<T>(BaseTypeRegistration registrationInfo)
        {
            var type = typeof(T);
            var keyedRegistrationInfo = registrationInfo as TypeRegistration<T>;
            Func<Expression, Expression> keyGetterExpressionBuilder;
            Func<Expression, Expression> factoryExpressionBuilder;
            MemberInfo keyMember = registrationInfo.KeyMember;
            if (keyedRegistrationInfo != null)
            {
                if(keyMember == null || keyedRegistrationInfo.KeyGetterExpressionBuilder == null || keyedRegistrationInfo.FactoryExpressionBuilder == null)
                {
                    throw new InvalidOperationException($"If you provide a custom {nameof(TypeRegistration<int>.KeyGetterExpressionBuilder)} "
                        + $"or {nameof(TypeRegistration<int>.KeyGetterExpressionBuilder)}, you must include both, as well as "
                        + $"the {nameof(TypeRegistration<int>.KeyMember)} parameter.");
                }
                keyGetterExpressionBuilder = keyedRegistrationInfo.KeyGetterExpressionBuilder;
                factoryExpressionBuilder = keyedRegistrationInfo.FactoryExpressionBuilder;
            } 
            else
            {
                try
                {
                    var constructor = type.GetConstructors().First(x => x.GetParameters().Count() == 0);
                    keyMember = keyMember != null ? keyMember : (MemberInfo)type.GetProperty("Id");
                    var constructInstance = Expression.New(constructor);

                    keyGetterExpressionBuilder = (parameter) => Expression.PropertyOrField(parameter, "Id");
                    factoryExpressionBuilder = (parameter) => Expression.MemberInit(constructInstance, Expression.Bind(keyMember, parameter));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Unable to create {nameof(keyGetterExpressionBuilder)} and {nameof(factoryExpressionBuilder)}  for type {type.FullName}. "
                      + $"Either give it a ulong property named `Id` or specify getter and setter with an IKeyedTypeRegistration.",
                        ex
                    );
                }
            }
            
            var registration = new TypeRegistration<T>()
            {
                Type = type,
                TypeId = registrationInfo.TypeId,
                AggregateId = registrationInfo.AggregateId,
                KeyMember = keyMember,
                KeyGetterExpressionBuilder = keyGetterExpressionBuilder,
                FactoryExpressionBuilder = factoryExpressionBuilder,
            };
            ByType<T>.Value = registration;
        }

        private bool TypeCanBeActivated(Type type)
        {
            return type.GetTypeInfo().IsValueType || type.GetConstructors().Any(x => x.GetParameters().Count() == 0);
        }

        private static class ByType<T>
        {
            public static TypeRegistration<T> Value;
        }
    }

    public class BaseTypeRegistration
    {
        public Type Type { get; set; }
        public ushort TypeId { get; set; }
        public ushort? AggregateId { get; set; }
        public MemberInfo KeyMember { get; set; }
    }

    public class TypeRegistration<T> : BaseTypeRegistration
    {
        // parameter of type T, should return ulong
        public Func<Expression, Expression> KeyGetterExpressionBuilder;

        // parameter of type ulong, should return T
        public Func<Expression, Expression> FactoryExpressionBuilder;
        
    }
    
}