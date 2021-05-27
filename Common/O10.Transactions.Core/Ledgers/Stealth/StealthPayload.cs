using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.Stealth.Transactions;

namespace O10.Transactions.Core.Ledgers.Stealth
{
    public class StealthPayload : PayloadBase<O10StealthTransactionBase>
    {
        public StealthPayload()
            : base()
        {

        }

        public StealthPayload(O10StealthTransactionBase transaction)
            : base(transaction)
        {

        }
    }
}
