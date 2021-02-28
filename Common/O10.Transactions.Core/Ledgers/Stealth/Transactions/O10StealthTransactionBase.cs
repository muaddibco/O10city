using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.Stealth.Internal;

namespace O10.Transactions.Core.Ledgers.Stealth.Transactions
{
    public abstract class O10StealthTransactionBase : StealthTransactionBase
    {
		/// <summary>
		/// P = Hs(r * A) * G + B where A is receiver's Public View Key and B is receiver's Public Spend Key
		/// </summary>
		public byte[] DestinationKey { get; set; }

		/// <summary>
		/// This is destination key of target that sender wants to authorize with
		/// </summary>
		public byte[] DestinationKey2 { get; set; }

		/// <summary>
		/// R = r * G. 'r' can be erased after transaction sent unless sender wants to proof he sent funds to particular destination address.
		/// </summary>
		public byte[] TransactionPublicKey { get; set; }
	
		/// <summary>
		/// C = x * G + I, where I is elliptic curve point representing assert id
		/// </summary>
		public byte[] AssetCommitment { get; set; }

		public BiometricProof BiometricProof { get; set; }
	}
}
