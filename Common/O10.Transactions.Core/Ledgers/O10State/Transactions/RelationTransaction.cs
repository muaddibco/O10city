using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public class RelationTransaction : O10StateTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_RelationRecord;

        /// <summary>
        /// A commitment to the Root Identity Asset ID of the user that this relation established for plus non-blinded commitment to the Asset ID of the group that this relations established with
        /// </summary>
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? RegistrationCommitment { get; set; }
    }
}
