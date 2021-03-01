namespace O10.Transactions.Core.DTOs
{
    public class SyncInfoDTO
    {
        public SyncInfoDTO(long height, byte[] hash)
        {
            Height = height;
            Hash = hash;
        }

        public long Height { get; }
        public byte[] Hash { get; set; }
    }
}
