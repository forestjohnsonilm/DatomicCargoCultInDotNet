using System.Reflection;
using System.Linq;

namespace DatomicNet.Core
{
    public class TypeRegistry
    {
        public TypeRegistry(params Assembly[] assemblies)
        {
            //assemblies.SelectMany(x => x.ExportedTypes.Where(x => x.Get))
        }
    }
}