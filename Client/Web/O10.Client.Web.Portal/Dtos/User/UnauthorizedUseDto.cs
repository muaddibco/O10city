using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Client.Web.Portal.Dtos.User
{
    public class UnauthorizedUseDto
    {
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey KeyImage { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey TransactionKey { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey DestinationKey { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey Target { get; set; }
    }
}
