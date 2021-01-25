using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Transactional.Internal
{
    public class InversedSurjectionProof
    {
        public InversedSurjectionProof()
        {
            Rs = new BorromeanRingSignature();
        }

        public byte[] AssetCommitment { get; set; }
        public BorromeanRingSignature Rs { get; set; }
    }
}
