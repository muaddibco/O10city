using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Registry
{
    public class RegistryConfidenceBlock : RegistryBlockBase
    {
        public override ushort PacketType => PacketTypes.Registry_ConfidenceBlock;

        public override ushort Version => 1;

        public byte[] BitMask { get; set; }

        public byte[] ConfidenceProof { get; set; }

        public byte[] ReferencedBlockHash { get; set; }
    }
}
