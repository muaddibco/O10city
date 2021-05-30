using O10.Core.Identity;
using O10.Transactions.Core.Enums;
using System.Collections.Generic;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public class DocumentRecordTransaction : O10StateTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_DocumentRecord;

        public IKey? DocumentHash { get; set; }

        /// <summary>
        /// Contains commitments to Asset IDs of Groups that document signer must belong to one of them
        /// </summary>
        public IEnumerable<IKey>? AllowedSignerGroupCommitments { get; set; }
    }
}
