using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Transactional.Internal
{
    public class AcceptedAsset
    {
        public byte[] AssetCommitment { get; set; }

        public SurjectionProof SurjectionProof { get; set; }
    }
}
