using O10.Core.Identity;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using System;

namespace O10.Transactions.Core.Ledgers.Synchronization
{
    public class SynchronizationPacket : OrderedPacketBase<SynchronizationTransactionBase, SingleSourceSignature>
    {
        public override LedgerType LedgerType => LedgerType.Synchronization;

        public IKey? HashPrev { get; set; }

        public DateTime ReportedTime { get; set; }
    }
}
