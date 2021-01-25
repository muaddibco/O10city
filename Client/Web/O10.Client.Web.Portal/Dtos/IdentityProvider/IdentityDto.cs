namespace O10.Client.Web.Portal.Dtos.IdentityProvider
{
    public class IdentityDto
    {
        public int NumberOfTransfers { get; set; }
        public string Id { get; set; }
        public string Description { get; set; }
        public IdentityAttributeDto[] Attributes { get; set; }
    }
}
