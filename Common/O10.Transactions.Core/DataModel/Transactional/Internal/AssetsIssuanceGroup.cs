namespace O10.Transactions.Core.DataModel.Transactional.Internal
{
    public class AssetsIssuanceGroup
    {
        public uint GroupId { get; set; }

        public AssetIssuance[] AssetIssuances { get; set; }
    }
}
