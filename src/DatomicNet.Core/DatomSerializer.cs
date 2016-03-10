using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace DatomicNet.Core
{


    public class DatomSerializer
    {
        public DatomSerializer(TypeRegistry typeRegistry)
        {
            _typeRegistry = typeRegistry;

        }

        private TypeRegistry _typeRegistry;

        private Dictionary<Type, IEnumerable<Func<object, IEnumerable<Datom>>>> _serializerCache;

        public Dictionary<Type, Func<IEnumerable<Datom>, object>> _deserializerCache;


    }
}
