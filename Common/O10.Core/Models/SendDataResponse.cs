using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Core.Models
{
    public class SendDataResponse
    {
        public bool Status { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] ExistingHash { get; set; }
    }
}
