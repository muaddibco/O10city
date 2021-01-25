namespace O10.Transactions.Core.DataModel
{
    public class RegistryCombinedBlockModel
    {
        public RegistryCombinedBlockModel(ulong height, byte[] content, byte[] hash)
        {
            Height = height;
            Content = content;
            Hash = hash;
        }

        public ulong Height { get; set; }
        public byte[] Content { get; set; }
        public byte[] Hash { get; set; }
    }
}
