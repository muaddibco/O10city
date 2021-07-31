using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Client.Web.DataContracts.User
{
    public class UserAttributeLastUpdateDto
    {
        public string Issuer { get; set; }

        public string AssetId { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey LastCommitment { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey LastTransactionKey { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey LastDestinationKey { get; set; }
    }
}
