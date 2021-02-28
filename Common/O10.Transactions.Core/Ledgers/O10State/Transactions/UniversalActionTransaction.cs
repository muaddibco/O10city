using O10.Transactions.Core.Actions;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public class UniversalActionTransaction : O10StateTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_UniversalTransport;

        public ActionBase? Action { get; set; }
    }
}
