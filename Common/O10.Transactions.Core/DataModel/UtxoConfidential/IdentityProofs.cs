using O10.Transactions.Core.DataModel.Stealth.Internal;
using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Stealth
{
    public class IdentityProofs : StealthTransactionBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => ActionTypes.Stealth_IdentityProofs;

        public EcdhTupleProofs EncodedPayload { get; set; }

        /// <summary>
        /// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
        /// </summary>
        public SurjectionProof OwnershipProof { get; set; }

        /// <summary>
        /// Contain Surjection Proofs to assets issued by Asset Issuer specified at EcdhTuple for checking eligibility purposes
        /// </summary>
        public SurjectionProof EligibilityProof { get; set; }

        /// <summary>
        /// Contain Surjection Proofs to the commitment of registration, where value of commitment is encoded using shared secret (one time secret key * Target Public Key)
        /// </summary>
        public SurjectionProof AuthenticationProof { get; set; }

		public byte[] TrustedRelation { get; set; }

		public AssociatedProofs[] AssociatedProofs { get; set; }
    }
}
