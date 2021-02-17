using O10.Core.Cryptography;

namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class AcceptedAsset
    {
        public byte[] AssetCommitment { get; set; }

        public SurjectionProof SurjectionProof { get; set; }
    }
}
