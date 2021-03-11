//using O10.Transactions.Core.Enums;
//using O10.Core.Cryptography;
//using O10.Core.Identity;

//namespace O10.Transactions.Core.Ledgers.Stealth
//{
//	public class TransitionCompromisedProofs : StealthTransactionBase
//	{
//		public override ushort Version => 1;

//		public override ushort TransactionType => TransactionTypes.Stealth_TransitionCompromisedProofs;

//		public EcdhTupleCA EcdhTuple { get; set; }

//		/// <summary>
//		/// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
//		/// </summary>
//		public SurjectionProof OwnershipProof { get; set; }

//		/// <summary>
//		/// Contain Surjection Proofs to assets issued by Asset Issuer specified at EcdhTuple for checking eligibility purposes
//		/// </summary>
//		public SurjectionProof EligibilityProof { get; set; }

//		/// <summary>
//		/// Key Image used in transaction that is compromised
//		/// </summary>
//		public byte[] CompromisedKeyImage { get; set; }

//        /// <summary>
//        /// Public Keys used in transaction that is compromised
//        /// </summary>
//        public IKey[] CompromisedPublicKeys { get; set; }

//		/// <summary>
//		/// Ring Signature of this message created using Public Keys and Key Image of compromised transaction
//		/// </summary>
//		public RingSignature[] CompromisedSignatures { get; set; }
//	}
//}
