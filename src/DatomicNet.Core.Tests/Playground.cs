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
        private Dictionary<Type, ushort> _dictionary;
        private readonly ITestOutputHelper _output;

        public Playground(ITestOutputHelper output)
        {
            _dictionary = new Dictionary<Type, ushort>();
            _output = output;
        }

        private interface IAsd
        {
            int Blah { get; set; }
        }

        private class Asd : IAsd
        {
            public int Blah { get; set; }
        }

        private class Holder<T> 
        {
            public Func<T> Get;
        }

        [Fact]
        public void TestInterfaceThing ()
        {
            var list = new List<Holder<IAsd>>();

            var holder = new Holder<Asd>() { Get = () => new Asd() { Blah = 2 } };

            var holder1 = Activator.CreateInstance(typeof(Holder<>).MakeGenericType(typeof(Asd))) as Holder<IAsd>;
            holder1.Get = () => new Asd() { Blah = 2 };

            var t = list[0].GetType();
            var t2 = list[0].Get().GetType();


            var t3 = holder1.GetType();
            var t4 = holder1.Get().GetType();

            var bp = 1;
        }



        [Fact]
        public void TestArrayPerf()
        {
            var count = 110;
            var dictionary = new Dictionary<int, int>();
            var array = new int[count];

            for(var i  = 0; i < count; i++)
            {
                array[i] = i.GetHashCode();
                dictionary.Add(i.GetHashCode(), i);
            }

            var iterations = 1000;

            var sw = new Stopwatch();
            sw.Start();
            long sum1 = 0;
            for (var i = 0; i < iterations; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    var hc = j.GetHashCode();
                    for (var k = 0; k < array.Length; k++)
                    {
                        if(array[k] == hc)
                        {
                            sum1 += i;
                            k = array.Length;
                        }
                    }
                }
            }
            sw.Stop();

            _output.WriteLine($"GetFromArray: {(long)(sw.ElapsedTicks / 10)}");
            Debug.WriteLine($"GetFromArray: {(long)(sw.ElapsedTicks / 10)}");

            var sw2 = new Stopwatch();
            sw2.Start();
            long sum2 = 0;
            for (var i = 0; i < iterations; i++)
            {
                for (var j = 0; j < count; j++)
                {
                    var hc = j.GetHashCode();
                    sum2 += dictionary[hc];
                }
            }
            sw2.Stop();

            _output.WriteLine($"GetFromDictionary: {(long)(sw2.ElapsedTicks / 10)}");
            Debug.WriteLine($"GetFromDictionary: {(long)(sw2.ElapsedTicks / 10)}");

            sum1.ShouldBeEquivalentTo(sum2);
        }


        [Fact]
        public void TestDictionaryPerf()
        {
            var types = new List<Type>
            {
                typeof(bool),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(ushort),
                typeof(uint),
                typeof(ulong),
                typeof(float),
                typeof(double)
            };

            for(var i = 0; i < types.Count; i++)
            {
                typeof(StaticGenericDictionary<>)
                    .MakeGenericType(types[i])
                    .GetProperty("Value")
                    .GetSetMethod()
                    .Invoke(null, new object[] { (ushort)i });

                _dictionary.Add(types[i], (ushort)i);
            }

            var count = 10000;

            var sw = new Stopwatch();
            sw.Start();
            long sum1 = 0;
            for(var i = 0; i < count; i++)
            {
                sum1 += GetFromDictionary<bool>();
                sum1 += GetFromDictionary<short>();
                sum1 += GetFromDictionary<int>();
                sum1 += GetFromDictionary<long>();
                sum1 += GetFromDictionary<ushort>();
                sum1 += GetFromDictionary<uint>();
                sum1 += GetFromDictionary<ulong>();
            }
            sw.Stop();

            _output.WriteLine($"GetFromDictionary: {(long)(sw.ElapsedTicks / count)}");
            Debug.WriteLine($"GetFromDictionary: {(long)(sw.ElapsedTicks / 1000)}");

            var sw2 = new Stopwatch();
            sw2.Start();
            long sum2 = 0;
            for (var i = 0; i < count; i++)
            {
                sum2 += GetFromStatic<bool>();
                sum2 += GetFromStatic<short>();
                sum2 += GetFromStatic<int>();
                sum2 += GetFromStatic<long>();
                sum2 += GetFromStatic<ushort>();
                sum2 += GetFromStatic<uint>();
                sum2 += GetFromStatic<ulong>();
            }
            sw2.Stop();

            _output.WriteLine($"GetFromStatic: {(long)(sw2.ElapsedTicks / count)}");
            Debug.WriteLine($"GetFromStatic: {(long)(sw2.ElapsedTicks / count)}");

            sum1.ShouldBeEquivalentTo(sum2);
        }

        ushort GetFromDictionary<T>()
        {
            return _dictionary[typeof(T)];
        }

        ushort GetFromStatic<T>()
        {
            return StaticGenericDictionary<T>.Value;
        }

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


    


    public static class StaticGenericDictionary<T>
    {
        public static ushort Value { get; set; }
    }
}