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
using System.Reflection;
using FluentAssertions;
using Google.Protobuf;

namespace DatomicNet.Core.Tests
{
    public class ProtobufSerializerTest
    {

        [Fact]
        public void SerializerSmokeTest()
        {
            var assemblies = new Assembly[] { typeof(ProtobufSerializerTest).GetTypeInfo().Assembly };
            var typeRegistry = new TypeRegistry((type) => typeof(IMessage).IsAssignableFrom(type), assemblies);
            var datomSerializer = new ProtobufDatomSerializer(typeRegistry, assemblies);
        }
        

    }

    public class InitialSchema : ISchemaChange
    {
        public IReadOnlyDictionary<uint, Func<IEnumerable<Datom>, IEnumerable<Datom>>> MapEntityStreamForType => null;

        public IReadOnlyDictionary<Type, uint> RegisterAggregates => new Dictionary<Type, uint>()
        {
            { typeof(TestAggregate), 1 },
        };

        public IReadOnlyDictionary<Type, uint> RegisterTypes => new Dictionary<Type, uint>()
        {
            { typeof(TestAggregate), 1 },
            { typeof(TransactionCategory), 2 },
            { typeof(OtherThing), 3 }
        };

        public bool RequiresReIndex => false;
        public long TransactionId => 1;
    }

}
