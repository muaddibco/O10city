using Benchmark.DataAccess;
using BenchmarkDotNet.Running;
using O10.Core.ExtensionMethods;
using System;

namespace Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var c = new JsonDeserializeBenchy();
            c.Init();

            var v1 = c.MemoryTextJson();
            var v2 = c.KeyTextJson();*/
            //BenchmarkRunner.Run<ByteArrayMemoryKeyBenchy>();
            //BenchmarkRunner.Run<JsonDeserializeBenchy>();
            BenchmarkRunner.Run<DataAccessBenchy>();
        }
    }
}
