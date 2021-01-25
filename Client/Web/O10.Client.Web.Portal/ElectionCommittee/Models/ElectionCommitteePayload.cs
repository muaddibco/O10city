using Newtonsoft.Json;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Core.Cryptography;
using O10.Core.Identity;

namespace O10.Client.Web.Portal.ElectionCommittee.Models
{
    public class ElectionCommitteePayload
    {
        [JsonConverter(typeof(KeyJsonConverter))]
        //[JsonProperty(ItemConverterType = typeof(KeyJsonConverter))]
        public IKey EcCommitment { get; set; }
        public SurjectionProof[] CandidatesProof { get; set; }
        public byte[] PartialBf { get; set; }
        public long PollId { get; set; }
    }
}
