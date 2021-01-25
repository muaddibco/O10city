namespace O10.Transactions.Core.DataModel.Transactional.Internal
{
    public class BlindedAssetsGroup
    {
        public uint GroupId { get; set; }

        public byte[][] AssetCommitments { get; set; }
    }
}
