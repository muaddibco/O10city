using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Transactions.Core.DTOs
{
    public class OutputSources
    {
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? DestinationKey { get; set; }
    }
}
