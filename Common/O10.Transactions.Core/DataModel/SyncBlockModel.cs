namespace O10.Transactions.Core.DataModel
{
    public class SyncBlockModel
    {
        public SyncBlockModel(ulong height, byte[] hash)
        {
            Height = height;
            Hash = hash;
        }

        public ulong Height { get; }
        public byte[] Hash { get; set; }
    }
}
