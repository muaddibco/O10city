using BenchmarkDotNet.Attributes;
using O10.Core.Identity;
using O10.Crypto.ConfidentialAssets;
using O10.Core.ExtensionMethods;
using System;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class ByteArrayMemoryKeyBenchy
    {
        private IIdentityKeyProvider _identityKeyProvider;
        private byte[] _key;

        [GlobalSetup]
        public void Init()
        {
            _identityKeyProvider = new DefaultKeyProvider();
            _key = CryptoHelper.GetRandomSeed();
        }

        [Benchmark]
        public void ByteArrayTest()
        {
            Processor.TakeAndProcess(_key);
        }

        [Benchmark]
        public void ByteMemoryTest()
        {
            Memory<byte> memory = new Memory<byte>(_key);
            Processor.TakeAndProcess(memory);
        }

        [Benchmark]
        public void KeyTest()
        {
            Processor.TakeAndProcess(_identityKeyProvider.GetKey(_key));
        }
    }
    public static class Processor
    {
        public static void TakeAndProcess(byte[] key)
        {
            var s = key.ToHexString();
        }
        public static void TakeAndProcess(Memory<byte> key)
        {
            var s = key.ToHexString();
        }
        public static void TakeAndProcess(IKey key)
        {
            var s = key.ToString();
        }
    }
}
