namespace O10.Client.Web.Portal.Dtos.ElectionCommittee
{
    public class RequestSelectionCommitmentDto
    {
        public string SelectionCommitment { get; set; }
        public CandidateCommitment[] CandidateCommitments { get; set; }
    }

    public class CandidateCommitment
    {
        public string Commitment { get; set; }
        public SurjectionProof SelectionProof { get; set; }
        public SurjectionProof[] IssuanceProofs { get; set; }
    }

    public class SurjectionProof
    {
        public string E { get; set; }
        public string S { get; set; }
    }
}
