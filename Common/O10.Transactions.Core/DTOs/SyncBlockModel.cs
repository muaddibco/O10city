namespace O10.Transactions.Core.DTOs
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
