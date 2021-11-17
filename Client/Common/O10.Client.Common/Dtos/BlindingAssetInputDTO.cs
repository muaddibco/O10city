using O10.Crypto.ConfidentialAssets;

namespace O10.Client.Common.Dtos
{
    public class BlindingAssetInputDTO
    {
        public BlindingAssetInputDTO()
        {

        }

        public BlindingAssetInputDTO(byte[] assetId)
        {
            AssetId = assetId;
            BlindingFactor = CryptoHelper.GetRandomSeed();
        }

        public byte[] AssetId { get; set; }
        public byte[] BlindingFactor { get; set; }
    }
}
