using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Stealth.Internal
{
	public class BiometricProof
	{
		public byte[] BiometricCommitment { get; set; }
		public SurjectionProof BiometricSurjectionProof { get; set; }

		public byte[] VerifierPublicKey { get; set; }
		public byte[] VerifierSignature { get; set; }
	}
}
