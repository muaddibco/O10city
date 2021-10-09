using BenchmarkDotNet.Attributes;
using O10.Crypto.ConfidentialAssets;
using O10.Core.ExtensionMethods;
using System.Numerics;
using O10.Core.Identity;

namespace Benchmark.BigIntegers
{
    [MemoryDiagnoser]
    public class BigIntegerBenchmark
    {
        private byte[][] _numbers;
        private IKey[] _keys;
        private BigInteger[] _bigIntegers;
        private readonly int _length = 100;

        [GlobalSetup]
        public void Setup()
        {
            _numbers = new byte[_length][];
            _keys = new IKey[_length];
            _bigIntegers = new BigInteger[_length];

            for (int i = 0; i < _length; i++)
            {
                _numbers[i] = CryptoHelper.GetRandomSeed();
                _keys[i] = new Key32(_numbers[i]);
                _bigIntegers[i] = new BigInteger(_numbers[i]);
            }
        }

        [Benchmark]
        public bool ByteArray()
        {
            bool eq = false;
            for (int i = 0; i < _length - 1; i++)
            {
                eq ^= _numbers[i].Equals32(_numbers[i + 1]);
            }

            return eq;
        }

        [Benchmark]
        public bool Keys()
        {
            bool eq = false;
            for (int i = 0; i < _length - 1; i++)
            {
                eq ^= _keys[i].Equals(_keys[i + 1]);
            }

            return eq;
        }

        [Benchmark]
        public bool BigInteger()
        {
            bool eq = false;
            for (int i = 0; i < _length - 1; i++)
            {
                eq ^= _bigIntegers[i] == _bigIntegers[i + 1];
            }

            return eq;
        }
    }
}
