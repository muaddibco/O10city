using O10.Client.Web.DataContracts.ElectionCommittee;

namespace O10.Client.Web.Portal.ElectionCommittee.Models
{
    public class PollResult
    {
        public Candidate Candidate { get; set; }
        public ulong Votes { get; set; }
    }
}
