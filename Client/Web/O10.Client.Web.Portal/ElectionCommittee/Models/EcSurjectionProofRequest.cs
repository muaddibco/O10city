using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Client.Web.Portal.ElectionCommittee.Models
{
    public class EcSurjectionProofRequest
    {
        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] EcCommitment { get; set; }

        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][] CandidateCommitments { get; set; }

        public int Index { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] PartialBlindingFactor { get; set; }
    }
}
