using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Core.Models
{
    public abstract class StealthSignedPacketBase : PacketBase
    {
        public IKey KeyImage { get; set; }

        public RingSignature[] Signatures { get; set; }

        public IKey[] PublicKeys { get; set; }
    }
}
