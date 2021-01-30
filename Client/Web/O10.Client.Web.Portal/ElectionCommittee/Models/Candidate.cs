using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Client.Web.Portal.ElectionCommittee.Models
{
    public class Candidate
    {
        public long CandidateId { get; set; }
        public string Name { get; set; }
        
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey AssetId { get; set; }
        public bool IsActive { get; set; }
    }
}
