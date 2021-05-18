using O10.Crypto.Models;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.O10State.Transactions;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class O10StatePacket : PacketBase<O10StatePayload, O10StateTransactionBase, SingleSourceSignature>
    {
        public O10StatePacket()
        {
            Payload = new O10StatePayload();
        }

        public override LedgerType LedgerType => LedgerType.O10State;
    }
}
