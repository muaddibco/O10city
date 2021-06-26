using Newtonsoft.Json;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    //[JsonConverter(typeof(TransactionJsonConverter), LedgerType.O10State)]
    public abstract class O10StateTransactionBase : SingleSourceTransactionBase
    {
    }
}
