using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.Ledgers.O10State
{
	public class DocumentSignRecord : TransactionalPacketBase
	{
		public override ushort Version => 1;

		public override ushort PacketType => PacketTypes.Transaction_DocumentSignRecord;

		public byte[] DocumentHash { get; set; }

		public ulong RecordHeight { get; set; }

        public byte[] KeyImage { get; set; }

        /// <summary>
        /// Commitment created from the Root Attribute
        /// </summary>
        public byte[] SignerCommitment { get; set; }
		
		public SurjectionProof EligibilityProof { get; set; }

		public byte[] Issuer { get; set; }

		/// <summary>
		/// Proof of relation of signer commitment to registration commitment at the group. This proof is built using AUX that is SHA3(document name | document content)
		/// </summary>
		public SurjectionProof SignerGroupRelationProof { get; set; }

		public byte[] GroupIssuer { get; set; }

		public byte[] SignerGroupCommitment { get; set; }

		public SurjectionProof SignerGroupProof { get; set; }

		public SurjectionProof SignerAllowedGroupsProof { get; set; }
	}
}
