using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;

namespace O10.Transactions.Core.Ledgers.Synchronization
{
    public class SynchronizationPacket : PacketBase<SynchronizationTransactionBase, SingleSourceSignature>
    {
        public override LedgerType LedgerType => LedgerType.Synchronization;
    }
}
