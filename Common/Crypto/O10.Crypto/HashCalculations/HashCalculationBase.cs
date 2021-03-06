﻿using HashLib;
using System;
using System.Runtime.InteropServices;
using O10.Core.Exceptions;
using O10.Core.HashCalculations;

namespace O10.Crypto.HashCalculations
{
    public abstract class HashCalculationBase : IHashCalculation
    {
        protected readonly IHash _hash;

        public abstract HashType HashType { get; }

        public virtual int HashSize => _hash.HashSize;

        public HashCalculationBase(IHash hash)
        {
            _hash = hash;
        }

        public byte[] CalculateHash(byte[] input)
        {
            lock (_hash)
            {
                HashResult hashRes = _hash.ComputeBytes(input);
                return hashRes.GetBytes();
            }
        }

        public byte[] CalculateHash(string input)
        {
            lock (_hash)
            {
                HashResult hashRes = _hash.ComputeString(input);
                return hashRes.GetBytes();
            }
        }

        public byte[] CalculateHash(Memory<byte> input)
        {
            lock (_hash)
            {
                if (MemoryMarshal.TryGetArray(input, out ArraySegment<byte> byteArray))
                {
                    HashResult hashRes = _hash.ComputeBytes(byteArray.Array);
                    return hashRes.GetBytes();
                }

                throw new FailedToMarshalToByteArrayException(nameof(input));
            }
        }
    }
}
