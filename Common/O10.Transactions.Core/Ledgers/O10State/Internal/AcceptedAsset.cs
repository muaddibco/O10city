using O10.Crypto.Models;

namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class AcceptedAsset
    {
        public byte[] AssetCommitment { get; set; }

        public SurjectionProof SurjectionProof { get; set; }
    }
}
