using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Stealth
{
    public class RevokeIdentity : StealthTransactionBase
	{
		public override ushort Version => 1;

		public override ushort BlockType => ActionTypes.Stealth_RevokeIdentity;

		/// <summary>
		/// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
		/// </summary>
		public SurjectionProof OwnershipProof { get; set; }

		/// <summary>
		/// Contain Surjection Proofs to assets issued by Asset Issuer specified at EcdhTuple for checking eligibility purposes
		/// </summary>
		public SurjectionProof EligibilityProof { get; set; }
	}
}
