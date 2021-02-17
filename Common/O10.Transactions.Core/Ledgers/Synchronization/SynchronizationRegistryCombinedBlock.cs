using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Synchronization
{
    public class SynchronizationRegistryCombinedBlock : SynchronizationBlockBase
    {
        public override ushort PacketType => PacketTypes.Synchronization_RegistryCombinationBlock;

        public override ushort Version => 1;

        public byte[][] BlockHashes { get; set; }
    }
}
