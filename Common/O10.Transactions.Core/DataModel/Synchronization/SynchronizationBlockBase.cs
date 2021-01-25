using System;

namespace O10.Transactions.Core.DataModel.Synchronization
{
    public abstract class SynchronizationBlockBase : LinkedPacketBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Synchronization;

        public DateTime ReportedTime { get; set; }
    }
}
