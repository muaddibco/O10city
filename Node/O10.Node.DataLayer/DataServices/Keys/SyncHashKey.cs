namespace O10.Node.DataLayer.DataServices.Keys
{
    public class SyncHashKey : IDataKey
    {
        public SyncHashKey(ulong syncBlockHeight, byte[] hash)
        {
            SyncBlockHeight = syncBlockHeight;
            Hash = hash;
        }

        public ulong SyncBlockHeight { get; set; }

        public byte[] Hash { get; set; }

    }
}
