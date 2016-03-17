using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatomicNet.Core
{
    public interface IKeyValueStore<T, TKey> where T : class
    {
        byte[] Get(byte[] value);

        IWriteBatch<T> GetWriteBatch();
        void Set(T value);

        IEnumerable<T> Range(TKey min, TKey max);
        IEnumerable<T> Range(TKey min, TKey max, bool descending);
    }

    public interface IWriteBatch<T> where T : class
    {
        IWriteBatch<T> Set(T value);
        void Commit();
    }
}
