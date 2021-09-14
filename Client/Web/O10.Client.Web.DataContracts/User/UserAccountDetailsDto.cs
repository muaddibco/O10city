using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Client.Web.DataContracts.User
{
    public class UserAccountDetailsDto
    {
        public long Id { get; set; }
        public string AccountInfo { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] PublicSpendKey { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] PublicViewKey { get; set; }

        public bool IsCompromised { get; set; }
        public bool IsAutoTheftProtection { get; set; }
        public string ConsentManagementHub { get; set; }
    }
}
