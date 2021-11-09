using O10.Crypto.Models;

namespace O10.Client.Web.DataContracts.ElectionCommittee
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

    public class SurjectionProofDTO
    {
        public string E { get; set; }
        public string S { get; set; }
    }
}
