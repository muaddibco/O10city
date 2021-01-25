using System;
using O10.Core.Architecture;

namespace O10.Core.HashCalculations
{
    [ExtensionPoint]
    public interface IHashCalculation
    {
        HashType HashType { get; }

        int HashSize { get; }

        byte[] CalculateHash(byte[] input);

        byte[] CalculateHash(Memory<byte> input);
    }
}
