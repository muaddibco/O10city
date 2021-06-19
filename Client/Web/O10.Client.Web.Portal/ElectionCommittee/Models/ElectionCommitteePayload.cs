using Newtonsoft.Json;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Client.Web.Portal.ElectionCommittee.Models
{
    public class ElectionCommitteePayload : PayloadBase
    {
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey EcCommitment { get; set; }

        public SurjectionProof[] CandidatesProof { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] PartialBf { get; set; }

        public long PollId { get; set; }
    }
}
