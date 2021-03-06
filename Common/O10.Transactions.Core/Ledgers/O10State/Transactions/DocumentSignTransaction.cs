﻿using Newtonsoft.Json;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.Serialization;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public class DocumentSignTransaction : O10StateTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_DocumentSignRecord;

        /// <summary>
        /// Hash of the signed document
        /// </summary>
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? DocumentHash { get; set; }

        /// <summary>
        /// The height of the aggregated registry at the time of signing. This is required in order to make sure 
        /// that signer had required permissions and valid identity at the time of signing
        /// </summary>
        public ulong RecordHeight { get; set; } // seems it is not required for locating the precise point of when exactly document was signed because the location of the signature on the timeline can be located using the hash of the transaction

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
        /// This proof is built using AUX that is SHA3(document name | document content)
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
