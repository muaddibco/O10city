namespace O10.Core.Cryptography
{
    public class RangeProof
    {
		public byte[][] D { get; set; } = new byte[64][]; // N/2 digit Pedersen commitments

        public BorromeanRingSignatureEx BorromeanRingSignature { get; set; }
    }
}
