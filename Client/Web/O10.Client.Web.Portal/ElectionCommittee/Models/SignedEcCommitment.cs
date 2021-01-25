namespace O10.Client.Web.Portal.ElectionCommittee.Models
{
    public class SignedEcCommitment
    {
        public byte[] EcCommitment { get; set; }
        public byte[] Signature { get; set; }
    }
}
