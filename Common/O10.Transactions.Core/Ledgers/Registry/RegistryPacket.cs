using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.Registry.Transactions;

namespace O10.Transactions.Core.Ledgers.Registry
{
    public class RegistryPacket : OrderedPacketBase<RegistryTransactionBase, SingleSourceSignature>
    {
        public override LedgerType LedgerType => LedgerType.Registry;
 
        public long SyncHeight { get; set; }
    }
}
