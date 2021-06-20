using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Crypto.Models;
using System;

namespace O10.Core.Cryptography
{
    [ExtensionPoint]
    public interface ISigningService
    {
        string Name { get; }

        IKey[] PublicKeys { get; }

        bool Verify<T>(IPayload<T> payload, SignatureBase signature) where T: TransactionBase;

        void Initialize(params byte[][] secretKeys);

        SignatureBase Sign<T>(PayloadBase<T> payload, object? args = null) where T: TransactionBase;

        byte[] Sign(Memory<byte> msg, object? args = null);
        byte[] Sign(string msg, object? args = null);
    }
}
