using System;
using O10.Core.HashCalculations;

namespace O10.Crypto.HashCalculations
{
    //[RegisterExtension(typeof(IHashCalculation), Lifetime = LifetimeManagement.TransientPerResolve)]
    public class NoneHashCalculation : IHashCalculation
    {
        public HashType HashType => HashType.None;

        public int HashSize => 0;

        public byte[] CalculateHash(Memory<byte> input) => null;

        public byte[] CalculateHash(byte[] input) => null;

        public byte[] CalculateHash(string input) => null;
    }
}
