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
    public class Playground
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

    }
}