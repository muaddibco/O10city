using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.Registry.Transactions;

namespace O10.Transactions.Core.Ledgers.Registry
{
    public class RegistryPayload : PayloadBase<RegistryTransactionBase>
    {
        public long Height { get; set; }

        public long SyncHeight { get; set; }
    }
}
