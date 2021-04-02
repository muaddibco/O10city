using O10.Core.Identity;
using System;

namespace O10.Transactions.Core.Ledgers.Synchronization.Transactions
{
    public abstract class SynchronizationTransactionBase : OrderedTransactionBase
    {
        public IKey? HashPrev { get; set; }

        public DateTime ReportedTime { get; set; }
    }
}
