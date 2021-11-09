using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    /// <summary>
    /// Transaction recording signature of a document. Using the hash of this transaction will be possible to determine exact point of time 
    /// when a document was signed and check whether signer was allowed to sign the document at the time of signing (Kirill, 2021-08-23)
    /// </summary>
    public class DocumentSignTransaction : O10StateTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_DocumentSignRecord;

        /// <summary>
        /// Hash of the transaction that records document to be signed. 
        /// </summary>
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[]? DocumentTransactionHash { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? KeyImage { get; set; } // KeyImage for checking for compromization (?)

        /// <summary>
        /// Commitment created from the Root Attribute
        /// </summary>
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? SignerCommitment { get; set; }

        public SurjectionProof? EligibilityProof { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? Issuer { get; set; }

        /// <summary>
        /// Proof of relation of signer commitment to registration commitment at the group. 
        /// This proof is built using AUX that is SHA3(document name | document content) (??, Kirill 2021/08/23)
        /// </summary>
        public SurjectionProof? SignerGroupRelationProof { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? GroupIssuer { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? SignerGroupCommitment { get; set; }

        public SurjectionProof? SignerGroupProof { get; set; }

        public SurjectionProof? SignerAllowedGroupsProof { get; set; }
    }
}
