using O10.Transactions.Core.DataModel.Stealth.Internal;

namespace O10.Transactions.Core.DataModel.Stealth
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
