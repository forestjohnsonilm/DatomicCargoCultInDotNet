﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatomicNet.Core
{
    //public interface IKeyValueStoreFactory<K, V>
    //{
    //    IKeyValueStore<K, V> Create(
    //            Func<K, byte[]> getKeyBytes, 
    //            Func<V, byte[]> getValueBytes, 
    //            Func<byte[], byte[], KeyValuePair<K,V>> getKeyValuePair
    //        );
    //}

    //public interface IKeyValueStore<K, V>
    //{
    //    V Get(K key);
    //    void Set(K key, V value);

    //    IEnumerable<KeyValuePair<K, V>> Range(K min, K max);
    //    IEnumerable<KeyValuePair<K, V>> Range(K min, K max, bool backwards);
    //}
    public interface IKeyValueStoreFactory<T, TKey> where T : class
    {
        IKeyValueStore<T, TKey> Create(
                int indexId,
                int keyWidth,
                Func<T, TKey> getKey,
                Func<TKey, byte[]> getKeyBytes, 
                Func<T, byte[]> getValueBytes, 
                Func<byte[], byte[], T> getTFromKeyValueBytes
            );
    }

    public interface IKeyValueStore<T, TKey> where T : class
    {
        T Get(T value);
        T Get(TKey key);

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
