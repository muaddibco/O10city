using O10.Transactions.Core.Ledgers.O10State.Internal;
using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class BlindAsset : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort PacketType => PacketTypes.Transaction_BlindAsset;

        public EncryptedAsset EncryptedAsset { get; set; }

        public SurjectionProof SurjectionProof { get; set; }
    }
}
