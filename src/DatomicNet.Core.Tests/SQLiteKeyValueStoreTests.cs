using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit;
using DatomicNet.Core;
using System.Diagnostics;
using System.IO;

namespace DatomicNet.Core.Tests
{
    public class SQLiteKeyValueStoreTests
    {

        [Fact]
        public void SQLiteKeyValueStoreSmokeTest()
        {
            var sqliteFactory = new SQLiteKeyValueStoreFactory<Datom, Datom>();

            var byEntity = sqliteFactory.Create(
                    1,
                    4 + 8 + 2 + 8 + 2,
                    (x) => x,
                    (x) => BitConverter.GetBytes(x.Type)
                        .Concat(BitConverter.GetBytes(x.Identity))
                        .Concat(BitConverter.GetBytes(x.Parameter))
                        .Concat(BitConverter.GetBytes(x.TransactionId))
                        .Concat(BitConverter.GetBytes((ushort)x.Action))
                        .ToArray(),

                    (x) => x.Value,

                    (key, value) => {
                        return new Datom(
                                BitConverter.ToUInt32(key, 0),
                                BitConverter.ToUInt64(key, 4),
                                BitConverter.ToUInt16(key, 4 + 8),
                                BitConverter.ToUInt64(key, 4 + 8 + 2),
                                (DatomAction)BitConverter.ToUInt16(key, 4 + 8 + 2 + 8),
                                value
                            );

                    }
                );

            byEntity.GetWriteBatch()
                .Set(new Datom(1, 1, 1, 1, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Set(new Datom(1, 1, 2, 1, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Set(new Datom(1, 1, 3, 1, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Set(new Datom(1, 1, 4, 1, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Commit();

            byEntity.GetWriteBatch()
                .Set(new Datom(1, 2, 1, 2, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Set(new Datom(1, 2, 2, 2, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Set(new Datom(1, 2, 3, 2, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Set(new Datom(1, 2, 4, 2, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Commit();

            byEntity.GetWriteBatch()
                .Set(new Datom(2, 2, 1, 3, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Set(new Datom(2, 2, 2, 3, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Set(new Datom(2, 2, 3, 3, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Set(new Datom(2, 2, 4, 3, DatomAction.Assertion, new byte[3] { 1, 4, 5 }))
                .Commit();

            var results = byEntity.Range(new Datom(1, 0, 0, 0, DatomAction.Unknown, new byte[0]), new Datom(2, 0, 0, 0, DatomAction.Unknown, new byte[0]));

            Assert.Equal(results.Count(), 8);
        }



    }

}
