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

namespace DatomicNet.Core.Tests
{
    public class SerializerTest
    {

        [Fact]
        public void TestDictionaryBehavesAsExpected()
        {
            var t = new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(1, "a"), new KeyValuePair<int, string>(1, "b") };
            Action a = () => t.ToDictionary(x => x.Key, x => x.Value);
            a.ShouldThrow<ArgumentException>();

            var t1 = new List<KeyValuePair<int, string>> { new KeyValuePair<int, string>(2, "a"), new KeyValuePair<int, string>(1, "b") };
            Action a1 = () => t1.ToDictionary(x => x.Key, x => x.Value);
            a1.ShouldNotThrow<Exception>();
        }

        [Fact]
        public void SerializerSmokeTest()
        {
            var assemblies = new Assembly[] { typeof(SerializerTest).GetTypeInfo().Assembly };
            var typeRegistry = new TypeRegistry(TypeShouldBeRegistered, assemblies);
        }

        private bool TypeShouldBeRegistered( Type type)
        {
            return type.GetTypeInfo().CustomAttributes.Any(x => x.AttributeType == typeof(TestModelAttribute));
        }

    }

    [TestModel]
    public class TestModel1
    {
        public string Prop1 { get; set; }
        public TestModel2 Prop2 { get; set; }
    }

    [TestModel]
    public class TestModel2
    {
        public int Prop1 { get; }
        public Guid Prop2 { get; }

        public TestModel2 (int prop1, Guid prop2)
        {
            Prop1 = prop1;
            Prop2 = prop2;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class TestModelAttribute : Attribute
    { }
}
