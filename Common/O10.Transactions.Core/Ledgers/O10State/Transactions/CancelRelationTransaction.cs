using O10.Core.Identity;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public class CancelRelationTransaction : O10StateTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_CancelRelation;
        public IKey RegistrationCommitment { get; set; }
    }
}
