using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.O10State.Transactions;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class O10StatePayload : PayloadBase<O10StateTransactionBase>
    {
        public O10StatePayload()
            : base()
        {

        }

        public O10StatePayload(O10StateTransactionBase transaction, long height)
            : base(transaction)
        {
            Height = height;
        }

        public long Height { get; set; }
    }
}
