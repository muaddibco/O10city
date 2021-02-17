using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.Ledgers.Stealth
{
	public class DocumentSignRequest : StealthTransactionBase
	{
		public override ushort Version => 1;

		public override ushort PacketType => PacketTypes.Stealth_DocumentSignRequest;

		/// <summary>
		/// Contains encrypted blinding factor of AssetCommitment: x` = x ^ (r * A). To decrypt receiver makes (R * a) ^ x` = x.
		/// </summary>
		public EcdhTupleProofs EcdhTuple { get; set; }

		/// <summary>
		/// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
		/// </summary>
		public SurjectionProof OwnershipProof { get; set; }

		/// <summary>
		/// Contain Surjection Proofs to assets issued by Asset Issuer specified at EcdhTuple for checking eligibility purposes
		/// </summary>
		public SurjectionProof EligibilityProof { get; set; }
		/// <summary>
		/// Commitment created from the Root Attribute
		/// </summary>

		/// <summary>
		/// Proof of relation of signer commitment to registration commitment at the group. This proof is built using AUX that is SHA3(document name | document content)
		/// </summary>
		public SurjectionProof SignerGroupRelationProof { get; set; }

		//public SurjectionProof SignerGroupNameSurjectionProof { get; set; }

		public byte[] AllowedGroupCommitment { get; set; }

		public SurjectionProof AllowedGroupNameSurjectionProof { get; set; }
	}
}
