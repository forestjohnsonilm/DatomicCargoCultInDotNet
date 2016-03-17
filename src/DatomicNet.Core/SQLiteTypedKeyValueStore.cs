//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using DatomicNet.Core;
//using Microsoft.Data.Sqlite;
//using System.Diagnostics;
//using System.IO;
//using Microsoft.Data;
//using System.Data;

//namespace DatomicNet.Core
//{
//    public class SQLiteKeyValueStoreFactory<T, TKey> : IKeyValueStoreFactory<T, TKey>, IDisposable where T : class
//    {
//        private readonly string _databaseFileName = "MyDatabase.sqlite";
//        private readonly SqliteConnection _connection;

//        public SQLiteKeyValueStoreFactory()
//        {
//            var dbFilePath = Path.Combine(FindDbFolder(Directory.GetCurrentDirectory()), _databaseFileName);
//            if (!File.Exists(dbFilePath))
//            {
//                File.Create(dbFilePath);
//            }

//            _connection = new SqliteConnection($"Data Source={dbFilePath};");
//            _connection.Open();
//        }

//        public IKeyValueStore<T, TKey> Create(
//                int indexId,
//                int keyWidthInBytes,
//                Func<T, TKey> getKey,
//                Func<TKey, byte[]> getKeyBytes,
//                Func<T, byte[]> getValueBytes,
//                Func<byte[], byte[], T> getTFromKeyValueBytes
//            )
//        {
//            var makeTable = _connection.CreateCommand();
//            makeTable.CommandText = $"CREATE TABLE IF NOT EXISTS kv_{indexId} ("
//                                    + $"key BINARY({keyWidthInBytes}) PRIMARY KEY ASC NOT NULL, "
//                                    + $"value BINARY NOT NULL"
//                                    + $") WITHOUT ROWID";

//            makeTable.ExecuteNonQuery();

//            return new SQLiteKeyValueStore<T, TKey>(
//                _connection,
//                indexId,
//                getKey,
//                getKeyBytes,
//                getValueBytes,
//                getTFromKeyValueBytes
//            );
//        }

//        private string FindDbFolder(string workingDirectory)
//        {
//            var directory = new DirectoryInfo(workingDirectory);
//            string appDataPath = null;
//            while (appDataPath == null && directory.Parent != null)
//            {
//                var appDataDir = directory.GetDirectories("db");
//                if (appDataDir.Any())
//                {
//                    appDataPath = appDataDir.First().FullName; ;
//                }
//                directory = directory.Parent;
//            }

//            return appDataPath;
//        }

//        public void Dispose()
//        {
//            if (_connection != null)
//            {
//                _connection.Dispose();
//            }
//        }
//    }

//    class SQLiteKeyValueStore<T, TKey> : IKeyValueStore<T, TKey> where T : class
//    {
//        private readonly SqliteConnection _connection;
//        private readonly int _indexId;
//        private readonly Func<T, TKey> _getKey;
//        private readonly Func<TKey, byte[]> _getKeyBytes;
//        private readonly Func<T, byte[]> _getValueBytes;
//        private readonly Func<byte[], byte[], T> _getTFromKeyValueBytes;

//        public SQLiteKeyValueStore(
//                SqliteConnection connection,
//                int indexId,
//                Func<T, TKey> getKey,
//                Func<TKey, byte[]> getKeyBytes,
//                Func<T, byte[]> getValueBytes,
//                Func<byte[], byte[], T> getTFromKeyValueBytes
//            )
//        {
//            _indexId = indexId;
//            _connection = connection;
//            _getKey = getKey;
//            _getKeyBytes = getKeyBytes;
//            _getValueBytes = getValueBytes;
//            _getTFromKeyValueBytes = getTFromKeyValueBytes;
//        }

//        public T Get(TKey key)
//        {
//            var keyBytes = _getKeyBytes(key);
//            using (var command = _connection.CreateCommand())
//            {
//                command.CommandText = $"SELECT value from kv_{ _indexId} where key = @key";
//                command.Parameters.Add("@key", SqliteType.Blob, keyBytes.Length).Value = keyBytes;
//                using (var reader = command.ExecuteReader(System.Data.CommandBehavior.SingleRow))
//                {
//                    if (reader.Read())
//                    {
//                        return _getTFromKeyValueBytes(keyBytes, reader.GetFieldValue<byte[]>(0));
//                    }
//                }
//            }

//            return null;
//        }

//        public T Get(T value)
//        {
//            return Get(_getKey(value));
//        }

//        public IEnumerable<T> Range(TKey min, TKey max)
//        {
//            return Range(min, max, false);
//        }

//        public IEnumerable<T> Range(TKey min, TKey max, bool descending)
//        {
//            var minBytes = _getKeyBytes(min);
//            var maxBytes = _getKeyBytes(max);
//            if (CompareByteArray(minBytes, maxBytes) == 1)
//            {
//                var swap = minBytes;
//                minBytes = maxBytes;
//                maxBytes = swap;
//            }

//            using (var command = _connection.CreateCommand())
//            {
//                command.CommandText = $"SELECT key, value from kv_{ _indexId} where key > @min and key < @max";
//                command.Parameters.Add("@min", SqliteType.Blob, minBytes.Length).Value = minBytes;
//                command.Parameters.Add("@max", SqliteType.Blob, maxBytes.Length).Value = maxBytes;
//                using (var reader = command.ExecuteReader())
//                {
//                    while (reader.Read())
//                    {
//                        yield return _getTFromKeyValueBytes(reader.GetFieldValue<byte[]>(0), reader.GetFieldValue<byte[]>(1));
//                    }
//                }
//            }

//        }

//        public IWriteBatch<T> GetWriteBatch()
//        {
//            return new SQLiteWriteBatch<T>(
//                _indexId,
//                _connection,
//                (value) => _getKeyBytes(_getKey(value)),
//                _getValueBytes
//            );
//        }

//        public void Set(T value)
//        {
//            GetWriteBatch().Set(value).Commit();
//        }

//        private int CompareByteArray(byte[] a, byte[] b)
//        {
//            var length = a.Length < b.Length ? a.Length : b.Length;
//            for (var i = 0; i < length; i++)
//            {
//                if (a[i] != b[i])
//                {
//                    return a[i] > b[i] ? 1 : -1;
//                }
//            }
//            return 0;
//        }
//    }

//    class SQLiteWriteBatch<T> : IWriteBatch<T> where T : class
//    {
//        private readonly int _indexId;
//        private readonly SqliteConnection _connection;
//        private readonly Func<T, byte[]> _getKeyBytes;
//        private readonly Func<T, byte[]> _getValueBytes;
//        private readonly List<T> _values;

//        public SQLiteWriteBatch(
//                int indexId,
//                SqliteConnection connection,
//                Func<T, byte[]> getKeyBytes,
//                Func<T, byte[]> getValueBytes
//            )
//        {
//            _indexId = indexId;
//            _getKeyBytes = getKeyBytes;
//            _getValueBytes = getValueBytes;
//            _connection = connection;
//            _values = new List<T>();
//        }

//        public IWriteBatch<T> Set(T value)
//        {
//            _values.Add(value);
//            return this;
//        }

//        public void Commit()
//        {
//            if (_values.Any())
//            {
//                var byteKeyValues = _values.Select(x => new KeyValuePair<byte[], byte[]>(_getKeyBytes(x), _getValueBytes(x)));
//                using (var command = _connection.CreateCommand())
//                {
//                    command.CommandText = $"INSERT OR REPLACE INTO kv_{_indexId} (key, value) VALUES (@key, @value)";
//                    command.Parameters.Add("@key", SqliteType.Blob, byteKeyValues.First().Key.Length);
//                    command.Parameters.Add("@value", SqliteType.Blob);
//                    using (var transaction = _connection.BeginTransaction())
//                    {
//                        command.Transaction = transaction;
//                        foreach (var byteKeyValue in byteKeyValues)
//                        {
//                            command.Parameters[0].Value = byteKeyValue.Key;
//                            command.Parameters[1].Size = byteKeyValue.Value.Length;
//                            command.Parameters[1].Value = byteKeyValue.Value;
//                            command.ExecuteNonQuery();
//                        }
//                        transaction.Commit();
//                    }
//                }
//            }
//        }
//    }
//}
