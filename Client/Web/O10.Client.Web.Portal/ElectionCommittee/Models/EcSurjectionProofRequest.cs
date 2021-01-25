namespace O10.Client.Web.Portal.ElectionCommittee.Models
{
    public class EcSurjectionProofRequest
    {
        public byte[] EcCommitment { get; set; }

        public byte[][] CandidateCommitments { get; set; }

        public int Index { get; set; }

        public byte[] PartialBlindingFactor { get; set; }
    }
}
