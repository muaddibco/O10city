namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class AcceptedAssetUnblinding
    {
        public byte[] AssetId { get; set; }

        public byte[] BlindingFactor { get; set; }
    }
}
