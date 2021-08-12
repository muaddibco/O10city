using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

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

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey Hash { get; set; }
    }
}
