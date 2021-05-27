using O10.Crypto.ConfidentialAssets;

namespace O10.Client.Common.Entities
{
    public class BlindingAssetInput
    {
        public BlindingAssetInput()
        {

        }

        public BlindingAssetInput(byte[] assetId)
        {
            AssetId = assetId;
            BlindingFactor = CryptoHelper.GetRandomSeed();
        }

        public byte[] AssetId { get; set; }
        public byte[] BlindingFactor { get; set; }
    }
}
