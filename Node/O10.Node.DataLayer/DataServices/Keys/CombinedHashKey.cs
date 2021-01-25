namespace O10.Node.DataLayer.DataServices.Keys
{
    public class CombinedHashKey : IDataKey
    {
        public CombinedHashKey(ulong combinedBlockHeight, byte[] hash)
        {
            CombinedBlockHeight = combinedBlockHeight;
            Hash = hash;
        }

        public ulong CombinedBlockHeight { get; set; }

        public byte[] Hash { get; set; }

    }
}
