using O10.Transactions.Core.Ledgers.Stealth.Internal;

namespace O10.Transactions.Core.Ledgers.Stealth
{
    public abstract class StealthTransactionBase : StealthBase
	{
		/// <summary>
		/// C = x * G + I, where I is elliptic curve point representing assert id
		/// </summary>
		public byte[] AssetCommitment { get; set; }

		public BiometricProof BiometricProof { get; set; }
	}
}
