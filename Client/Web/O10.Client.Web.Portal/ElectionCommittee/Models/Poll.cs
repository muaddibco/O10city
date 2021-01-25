using O10.Core.Identity;
using System.Collections.Generic;

namespace O10.Client.Web.Portal.ElectionCommittee.Models
{
    public class Poll
    {
        public long PollId { get; set; }
        public string Name { get; set; }
        public PollState State { get; set; }
        public string Issuer { get; set; }
        public List<Candidate> Candidates { get; set; }
    }
}