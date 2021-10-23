using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Core.Models
{
    public class PacketHashResponse
    {
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] Hash { get; set; }
    }
}
