﻿namespace O10.Client.Web.DataContracts.User
{
    public class VoteDto
    {
        public long PollId { get; set; }

        public string[] CandidateAssetIds { get; set; }
        public string SelectedAssetId{ get; set; }
    }
}
