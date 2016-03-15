using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;


namespace DatomicNet.Core
{
    public interface IDatomSerializer
    {
        IEnumerable<T> DeserializeMany<T>(IEnumerable<Datom> datoms);

        T Deserialize<T>(IEnumerable<Datom> datoms);

        IEnumerable<Datom> Serialize<T>(IEnumerable<T> objects);

        IEnumerable<Datom> Serialize<T>(T @object);
    }
}
