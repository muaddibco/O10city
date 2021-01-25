using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Core.Models;
using System;

namespace O10.Core.Cryptography
{
    [ExtensionPoint]
    public interface ISigningService
    {
        string Name { get; }

        IKey[] PublicKeys { get; }

        bool Verify(IPacket packet);

        void Initialize(params byte[][] secretKeys);

        void Sign(IPacket packet, object args = null);
        byte[] Sign(Memory<byte> msg, object args = null);
    }
}
