using System;

namespace O10.Transactions.Core.Ledgers.Synchronization.Transactions
{
    public abstract class SynchronizationTransactionBase : OrderedTransactionBase
    {
        public byte[] HashPrev { get; set; }

        public DateTime ReportedTime { get; set; }
    }
}
