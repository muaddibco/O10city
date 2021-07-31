namespace O10.Client.Web.DataContracts.ElectionCommittee
{
    public class SetPollStateRequest
    {
        public PollState State { get; set; }
        public long SourceAccountId { get; set; }
    }
}
