using BenchmarkDotNet.Running;
using O10.Core.ExtensionMethods;
using System;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<ByteArrayMemoryKeyBenchy>();
            BenchmarkRunner.Run<JsonDeserializeBenchy>();
        }
    }
}
