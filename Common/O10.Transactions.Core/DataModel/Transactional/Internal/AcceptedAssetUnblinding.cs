namespace O10.Transactions.Core.DataModel.Transactional.Internal
{
    public class AcceptedAssetUnblinding
    {
        public byte[] AssetId { get; set; }

        public byte[] BlindingFactor { get; set; }
    }
}
