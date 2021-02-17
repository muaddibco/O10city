namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class AssetsIssuanceGroup
    {
        public uint GroupId { get; set; }

        public AssetIssuance[] AssetIssuances { get; set; }
    }
}
