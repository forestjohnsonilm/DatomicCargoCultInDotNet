using System.Reflection;
using System.Linq;
using System;
using System.Collections.Generic;

namespace DatomicNet.Core
{
    public class TypeRegistry
    {
        public TypeRegistry(
                Func<Assembly, Type, bool> registerTypePredicate,
                params Assembly[] assemblies
            )
        {
            var test = assemblies.SelectMany(assembly => assembly.ExportedTypes.Where(type => registerTypePredicate(assembly, type)))
                .Select(x => x);
        }
    }
}