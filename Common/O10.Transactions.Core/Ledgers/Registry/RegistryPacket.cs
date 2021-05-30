using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.Registry.Transactions;

namespace O10.Transactions.Core.Ledgers.Registry
{
    public class RegistryPacket : PacketBase<RegistryPayload, RegistryTransactionBase, SingleSourceSignature>
    {
        public RegistryPacket()
        {
            Payload = new RegistryPayload();
        }

        public RegistryPacket(RegistryTransactionBase transaction)
        {
            Payload = new RegistryPayload { Transaction = transaction };
        }

        public override LedgerType LedgerType => LedgerType.Registry;
    }
}
