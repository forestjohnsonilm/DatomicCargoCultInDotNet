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
using Google.Protobuf.Reflection;

namespace DatomicNet.Core.Tests
{
    public class ProtobufSerializerTest
    {

        private TypeRegistry _typeRegistry;
        private ProtobufDatomSerializer _datomSerializer;
        private ulong _aggregateIdentity = 1;
        private ulong _categoryId1 = 10;
        private ulong _categoryId2 = 20;
        private ulong _otherThingId = 3;
        private ulong _txId = 1;

        public ProtobufSerializerTest()
        {
            var assemblies = new Assembly[] { typeof(ProtobufSerializerTest).GetTypeInfo().Assembly };
            _typeRegistry = new TypeRegistry((type) => typeof(IMessage).IsAssignableFrom(type), assemblies);
            _datomSerializer = new ProtobufDatomSerializer(_typeRegistry, assemblies);
        }

        [Fact]
        public void DeserializeTest()
        {
            var datoms = GetTestDatoms();

            var result = _datomSerializer.Deserialize<TestAggregate>(datoms);

            result.ShouldBeEquivalentTo(GetTestAggregate());

        }
        
        //message TestAggregate {
        //  uint64 id = 1;
        //  uint32 categoryId = 2;
        //  repeated TransactionCategory categories = 3;
        //  string description = 7;
        //  OtherThing otherThing = 9;
        //}

        //message TransactionCategory {
        //  uint64 id = 1;
        //  bool isGreat = 2;
        //  string name = 3;
        //}

        //message OtherThing {
        //  uint64 id = 1;
        //  float h = 2;
        //  float s = 3;
        //  float v = 4;
        //}

        private TestAggregate GetTestAggregate()
        {
            var toReturn = new TestAggregate()
            {
                Id = _aggregateIdentity,
                OtherThing = new OtherThing()
                {
                    Id = _otherThingId,
                    H = 0.1f,
                    S = 0.2f,
                    V = 0.3f,
                }
            };

            toReturn.Categories.Add(new TransactionCategory()
            {
                Id = _categoryId1,
                IsGreat = true,
                Name = "_categoryId1"
            });
            toReturn.Categories.Add(new TransactionCategory()
            {
                Id = _categoryId2,
                IsGreat = true,
                Name = "_categoryId2"
            });

            return toReturn;
        }

        private IEnumerable<Datom> GetTestDatoms ()
        {
            //yield return new Datom(
            //    _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
            //    _aggregateIdentity,
            //    _typeRegistry.IdByType[typeof(TestAggregate)],
            //    _aggregateIdentity,
            //    (ushort)TestAggregate.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TestAggregate.Id)}")).FieldNumber,
            //    0,
            //    BitConverter.GetBytes(_aggregateIdentity),
            //    _txId,
            //    DatomAction.Assertion
            //);
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                (ushort)TestAggregate.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TestAggregate.Categories)}")).FieldNumber,
                0,
                BitConverter.GetBytes(_categoryId1),
                _txId,
                DatomAction.Assertion
            );
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                (ushort)TestAggregate.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TestAggregate.Categories)}")).FieldNumber,
                1,
                BitConverter.GetBytes(_categoryId2),
                _txId,
                DatomAction.Assertion
            );
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                (ushort)TestAggregate.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TestAggregate.OtherThing)}")).FieldNumber,
                0,
                BitConverter.GetBytes(_otherThingId),
                _txId,
                DatomAction.Assertion
            );



            //yield return new Datom(
            //    _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
            //    _aggregateIdentity,
            //    _typeRegistry.IdByType[typeof(TransactionCategory)],
            //    _categoryId1,
            //    (ushort)TransactionCategory.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TransactionCategory.Id)}")).FieldNumber,
            //    0,
            //    BitConverter.GetBytes(_categoryId1),
            //    _txId,
            //    DatomAction.Assertion
            //);
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(TransactionCategory)],
                _categoryId1,
                (ushort)TransactionCategory.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TransactionCategory.IsGreat)}")).FieldNumber,
                0,
                BitConverter.GetBytes(true),
                _txId,
                DatomAction.Assertion
            );
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(TransactionCategory)],
                _categoryId1,
                (ushort)TransactionCategory.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TransactionCategory.Name)}")).FieldNumber,
                0,
                Encoding.UTF8.GetBytes("_categoryId1"),
                _txId,
                DatomAction.Assertion
            );

            //yield return new Datom(
            //    _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
            //    _aggregateIdentity,
            //    _typeRegistry.IdByType[typeof(TransactionCategory)],
            //    _categoryId2,
            //    (ushort)TransactionCategory.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TransactionCategory.Id)}")).FieldNumber,
            //    0,
            //    BitConverter.GetBytes(_categoryId2),
            //    _txId,
            //    DatomAction.Assertion
            //);
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(TransactionCategory)],
                _categoryId2,
                (ushort)TransactionCategory.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TransactionCategory.IsGreat)}")).FieldNumber,
                0,
                BitConverter.GetBytes(true),
                _txId,
                DatomAction.Assertion
            );
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(TransactionCategory)],
                _categoryId2,
                (ushort)TransactionCategory.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(TransactionCategory.Name)}")).FieldNumber,
                0,
                Encoding.UTF8.GetBytes("_categoryId2"),
                _txId,
                DatomAction.Assertion
            );

            //yield return new Datom(
            //    _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
            //    _aggregateIdentity,
            //    _typeRegistry.IdByType[typeof(OtherThing)],
            //    _otherThingId,
            //    (ushort)OtherThing.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(OtherThing.Id)}")).FieldNumber,
            //    0,
            //    BitConverter.GetBytes(_otherThingId),
            //    _txId,
            //    DatomAction.Assertion
            //);
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(OtherThing)],
                _otherThingId,
                (ushort)OtherThing.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(OtherThing.H)}")).FieldNumber,
                0,
                BitConverter.GetBytes(0.1f),
                _txId,
                DatomAction.Assertion
            );
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(OtherThing)],
                _otherThingId,
                (ushort)OtherThing.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(OtherThing.S)}")).FieldNumber,
                0,
                BitConverter.GetBytes(0.2f),
                _txId,
                DatomAction.Assertion
            );
            yield return new Datom(
                _typeRegistry.AggregateIdByType[typeof(TestAggregate)],
                _aggregateIdentity,
                _typeRegistry.IdByType[typeof(OtherThing)],
                _otherThingId,
                (ushort)OtherThing.Descriptor.Fields.InFieldNumberOrder().First(Named($"{nameof(OtherThing.V)}")).FieldNumber,
                0,
                BitConverter.GetBytes(0.3f),
                _txId,
                DatomAction.Assertion
            );
        }

        private Func<FieldDescriptor, bool> Named (string name)
        {
            return x =>
            {
                var titleCaseName = $"{x.Name.Substring(0, 1).ToUpper()}{x.Name.Substring(1)}";
                return name == titleCaseName;
            };
        }
    }

    public class InitialSchema : ISchemaChange
    {
        public IReadOnlyDictionary<ushort, Func<IEnumerable<Datom>, IEnumerable<Datom>>> MapEntityStreamForType => null;

        public IReadOnlyDictionary<Type, ushort> RegisterAggregates => new Dictionary<Type, ushort>()
        {
            { typeof(TestAggregate), 1 },
        };

        public IReadOnlyDictionary<Type, ushort> RegisterTypes => new Dictionary<Type, ushort>()
        {
            { typeof(TestAggregate), 1 },
            { typeof(TransactionCategory), 2 },
            { typeof(OtherThing), 3 }
        };

        public bool RequiresReIndex => false;
        public long TransactionId => 1;
    }

}
