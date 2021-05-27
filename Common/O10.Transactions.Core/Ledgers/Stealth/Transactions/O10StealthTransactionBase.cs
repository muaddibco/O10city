using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.Stealth.Internal;
using System;

namespace O10.Transactions.Core.Ledgers.Stealth.Transactions
{
    public abstract class O10StealthTransactionBase : StealthTransactionBase
    {
		/// <summary>
		/// P = Hs(r * A) * G + B where A is receiver's Public View Key and B is receiver's Public Spend Key
		/// </summary>
		public IKey? DestinationKey { get; set; }

		/// <summary>
		/// This is destination key of target that sender wants to authorize with
		/// </summary>
		public IKey? DestinationKey2 { get; set; }

		/// <summary>
		/// R = r * G. 'r' can be erased after transaction sent unless sender wants to proof he sent funds to particular destination address.
		/// </summary>
		public IKey? TransactionPublicKey { get; set; }
	
		/// <summary>
		/// C = x * G + I, where I is elliptic curve point representing asset id
		/// </summary>
		public IKey? AssetCommitment { get; set; }

		/// <summary>
		/// Contain Surjection Proofs to assets of source transaction which outputs were used to compose current one
		/// </summary>
		public SurjectionProof? OwnershipProof { get; set; }

		public BiometricProof? BiometricProof { get; set; }
		
		/// <summary>
		/// Hash of the data with required proofs that was transferred off-chain
		/// </summary>
		public Memory<byte> ProofsHash { get; set; }
	}
}
