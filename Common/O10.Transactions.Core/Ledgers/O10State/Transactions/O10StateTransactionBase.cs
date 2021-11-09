using O10.Crypto.Models;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    //[JsonConverter(typeof(TransactionJsonConverter), LedgerType.O10State)]
    public abstract class O10StateTransactionBase : SingleSourceTransactionBase
    {
    }
}
