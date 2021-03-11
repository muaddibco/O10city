using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public class DocumentSignTransaction : O10StateTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_DocumentSignRecord;
	
		/// <summary>
		/// Hash of the signed document
		/// </summary>
		public IKey? DocumentHash { get; set; }

		/// <summary>
		/// The height of the aggregated registry at the time of signing. This is required in order to make sure 
		/// that signer had required permissions and valid identity at the time of signing
		/// </summary>
		public ulong RecordHeight { get; set; }

		public IKey? KeyImage { get; set; }

		/// <summary>
		/// Commitment created from the Root Attribute
		/// </summary>
		public IKey? SignerCommitment { get; set; }

		public SurjectionProof? EligibilityProof { get; set; }

		public IKey? Issuer { get; set; }

		/// <summary>
		/// Proof of relation of signer commitment to registration commitment at the group. 
		/// This proof is built using AUX that is SHA3(document name | document content)
		/// </summary>
		public SurjectionProof? SignerGroupRelationProof { get; set; }

		public IKey? GroupIssuer { get; set; }

		public IKey? SignerGroupCommitment { get; set; }

		public SurjectionProof? SignerGroupProof { get; set; }

		public SurjectionProof? SignerAllowedGroupsProof { get; set; }
	}
}
