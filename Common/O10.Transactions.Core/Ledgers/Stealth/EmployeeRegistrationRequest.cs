//using O10.Transactions.Core.Ledgers.Stealth.Internal;
//using O10.Transactions.Core.Enums;
//using O10.Core.Cryptography;

//namespace O10.Transactions.Core.Ledgers.Stealth
//{
//    /// <summary>
//    /// This transaction does not transfer ownership of asset but only sends proofs to some State account
//    /// </summary>
//    public class EmployeeRegistrationRequest : StealthTransactionBase
//	{
//        public override ushort Version => 1;

//        public override ushort TransactionType => TransactionTypes.Stealth_EmployeeReqistrationRequest;

//        /// <summary>
//        /// Contains encrypted blinding factor of AssetCommitment: x` = x ^ (r * A). To decrypt receiver makes (R * a) ^ x` = x.
//        /// </summary>
//        public EcdhTupleProofs EcdhTuple { get; set; }

//        /// <summary>
//        /// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
//        /// </summary>
//        public SurjectionProof OwnershipProof { get; set; }

//        /// <summary>
//        /// Contain Surjection Proofs to assets issued by Asset Issuer specified at EcdhTuple for checking eligibility purposes
//        /// </summary>
//        public SurjectionProof EligibilityProof { get; set; }

//		public AssociatedProofs[] AssociatedProofs { get; set; }

//        public byte[] GroupCommitment { get; set; }

//        public SurjectionProof GroupSurjectionProof { get; set; }
//    }
//}
