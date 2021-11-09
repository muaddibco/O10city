using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;
using System.Collections.Generic;

namespace O10.Crypto.Models
{
    public class StealthSignature : SignatureBase
    {
        [JsonProperty(ItemConverterType = typeof(KeyJsonConverter))]
        public IEnumerable<IKey>? Sources { get; set; }

        public IEnumerable<RingSignature>? Signature { get; set; }
    }
}
