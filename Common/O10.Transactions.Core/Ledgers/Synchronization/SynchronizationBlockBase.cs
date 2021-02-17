using System;

namespace O10.Transactions.Core.Ledgers.Synchronization
{
    public abstract class SynchronizationBlockBase : LinkedPacketBase
    {
        public override ushort LedgerType => (ushort)Enums.LedgerType.Synchronization;

        public DateTime ReportedTime { get; set; }
    }
}
