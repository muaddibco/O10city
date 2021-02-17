using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Synchronization
{
    public class SynchronizationConfirmedBlock : SynchronizationBlockBase
    {
        public override ushort LedgerType => (ushort)Enums.LedgerType.Synchronization;

        public override ushort PacketType => PacketTypes.Synchronization_ConfirmedBlock;

        public override ushort Version => 1;

        public ushort Round { get; set; }

        public byte[][] Signatures { get; set; }

        public byte[][] PublicKeys { get; set; }
    }
}
