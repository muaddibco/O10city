namespace O10.Client.Web.Portal.Dtos.User
{
    public class RelationsProofsDto : UserAttributeTransferDto
    {
        public bool WithBiometricProof { get; set; }
        public bool WithKnowledgeProof { get; set; }
        public GroupRelationDto[] Relations { get; set; }
    }
}
