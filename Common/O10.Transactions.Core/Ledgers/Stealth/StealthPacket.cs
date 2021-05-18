using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.Stealth.Transactions;

namespace O10.Transactions.Core.Ledgers.Stealth
{
    public class StealthPacket : PacketBase<StealthPayload, O10StealthTransactionBase, StealthSignature>
    {
        public override LedgerType LedgerType => LedgerType.Stealth;
    }
}
