using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;

namespace DatomicNet.Core
{
    public class TypeRegistry
    {
        public readonly IReadOnlyDictionary<ushort, Type> TypesById;
        public readonly IReadOnlyDictionary<Type, ushort> IdByType;
        public readonly IReadOnlyDictionary<ushort, Type> AggregateTypesById;
        public readonly IReadOnlyDictionary<Type, ushort> AggregateIdByType;

        public TypeRegistry(
                Func<Type, bool> registerTypePredicate,
                params Assembly[] assemblies
            )
        {
            var registeredTypes = assemblies
                .SelectMany(assembly => assembly.ExportedTypes.Where(type => registerTypePredicate(type)));

            var schemaChangeTypes = assemblies
                .SelectMany(assembly => assembly.ExportedTypes.Where(type => typeof(ISchemaChange).IsAssignableFrom(type)));

            var errorSchemaChangeTypes = schemaChangeTypes.Where(x => !TypeCanBeActivated(x));

            if (errorSchemaChangeTypes.Any())
            {
                throw new InvalidOperationException($"ISchemaChanges {string.Join(", ", errorSchemaChangeTypes.Select(x => x.Name))}"
                                                  + $" are invalid because they do not have a default constructor.");
            }

            var schemaChanges = schemaChangeTypes.Select(x => (ISchemaChange)Activator.CreateInstance(x));
            var aggregateIds = schemaChanges.SelectMany(x => x.RegisterAggregates);
            var typeIds = schemaChanges.SelectMany(x => x.RegisterTypes);

            var errors = typeIds.GroupBy(x => x.Value)
                .Where(x => x.Count() > 1)
                .Select(x => $"Type Id `{x.First().Value}` is associated with multiple types: [{string.Join(", ", x.Select(y => y.Key.FullName))}]");
            if (errors.Any())
            {
                throw new InvalidOperationException($"Invalid Schema: {string.Join(", ", errors)}.");
            }
            TypesById = typeIds.ToDictionary(x => x.Value, x => x.Key);

            errors = typeIds.GroupBy(x => x.Key)
                .Where(x => x.Count() > 1)
                .Select(x => $"Type `{x.First().Key.FullName}` is associated with multiple ids: [{string.Join(", ", x.Select(y => y.Value))}]");
            if (errors.Any())
            {
                throw new InvalidOperationException($"Invalid Schema: {string.Join(", ", errors)}.");
            }
            IdByType = typeIds.ToDictionary(x => x.Key, x => x.Value);

            errors = aggregateIds.GroupBy(x => x.Value)
                .Where(x => x.Count() > 1)
                .Select(x => $"Aggregate Type Id `{x.First().Value}` is associated with multiple types: [{string.Join(", ", x.Select(y => y.Key.FullName))}]");
            if (errors.Any())
            {
                throw new InvalidOperationException($"Invalid Schema: {string.Join(", ", errors)}.");
            }
            AggregateTypesById = aggregateIds.ToDictionary(x => x.Value, x => x.Key);

            errors = aggregateIds.GroupBy(x => x.Key)
                .Where(x => x.Count() > 1)
                .Select(x => $"Type `{x.First().Key.FullName}` is associated with multiple aggregate type ids: [{string.Join(", ", x.Select(y => y.Value))}]");
            if (errors.Any())
            {
                throw new InvalidOperationException($"Invalid Schema: {string.Join(", ", errors)}.");
            }
            AggregateIdByType = aggregateIds.ToDictionary(x => x.Key, x => x.Value);

            var registeredButNotGivenId = registeredTypes.Where(x => !IdByType.ContainsKey(x)).Select(x => x.FullName);
            if (registeredButNotGivenId.Any())
            {
                throw new InvalidOperationException(
                    $"Registered types [{string.Join(", ", registeredButNotGivenId)}] don't have Type Ids. "
                  + $"Create an ISchemaChange with a default RegisterTypes that includes them."
                );
            }
            var givenIdButNotRegistered = IdByType.Where(x => !registeredTypes.Contains(x.Key)).Select(x => x.Key.FullName);
            if (givenIdButNotRegistered.Any())
            {
                throw new InvalidOperationException(
                    $"Types [{string.Join(", ", givenIdButNotRegistered)}] were mentioned in an ISchemaChange but not registered. "
                  + $"make sure your registerTypePredicate matches them and you include the assemblies in which they are declared."
                );
            }
        }

        private bool TypeCanBeActivated(Type type)
        {
            return type.GetTypeInfo().IsValueType || type.GetConstructors().Any(x => x.GetParameters().Count() == 0);
        }
    }
}