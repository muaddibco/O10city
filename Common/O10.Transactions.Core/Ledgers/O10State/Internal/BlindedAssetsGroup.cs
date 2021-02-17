namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class BlindedAssetsGroup
    {
        public uint GroupId { get; set; }

        public byte[][] AssetCommitments { get; set; }
    }
}
