using System;
using O10.Core.Identity;

namespace O10.Core.Models
{
    public abstract class SignedPacketBase : PacketBase
    {
        public ulong BlockHeight { get; set; }

        public IKey Signer { get; set; }

        public Memory<byte> Signature { get; set; }
    }
}
