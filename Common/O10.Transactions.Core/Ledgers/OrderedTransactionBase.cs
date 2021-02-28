using O10.Crypto.Models;

namespace O10.Transactions.Core.Ledgers
{
    public abstract class OrderedTransactionBase : SingleSourceTransactionBase
    {
        public long Height { get; set; }

    }
}
