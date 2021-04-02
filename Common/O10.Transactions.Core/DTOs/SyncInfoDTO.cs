using O10.Core.Identity;

namespace O10.Transactions.Core.DTOs
{
    public class SyncInfoDTO
    {
        public SyncInfoDTO(long height, IKey hash)
        {
            Height = height;
            Hash = hash;
        }

        public long Height { get; }
        public IKey Hash { get; set; }
    }
}
