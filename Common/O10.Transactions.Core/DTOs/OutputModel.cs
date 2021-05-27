using Newtonsoft.Json;
using O10.Core.Identity;

namespace O10.Transactions.Core.DTOs
{
    public class OutputModel
    {
        [JsonConverter(typeof(IKey))]
        public IKey? Commitment { get; set; }

        [JsonConverter(typeof(IKey))]
        public IKey? DestinationKey { get; set; }
    }
}
