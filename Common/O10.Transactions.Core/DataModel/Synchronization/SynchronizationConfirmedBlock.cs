using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel.Synchronization
{
    public class SynchronizationConfirmedBlock : SynchronizationBlockBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Synchronization;

        public override ushort BlockType => ActionTypes.Synchronization_ConfirmedBlock;

        public override ushort Version => 1;

        public ushort Round { get; set; }

        public byte[][] Signatures { get; set; }

        public byte[][] PublicKeys { get; set; }
    }
}
