using O10.Client.Web.Portal.ElectionCommittee.Models;

namespace O10.Client.Web.Portal.Dtos.ElectionCommittee
{
    public class SetPollStateRequest
    {
        public PollState State { get; set; }
        public long SourceAccountId { get; set; }
    }
}
