namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class AssetsGroup
    {
        public uint GroupId { get; set; }

        public byte[][] AssetIds { get; set; }

        public ulong[] AssetAmounts { get; set; }
    }
}
