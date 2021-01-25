namespace O10.Client.Web.Portal.Dtos.User
{
    public class UserAttributeDto
    {
        public long UserAttributeId { get; set; }

        public string SchemeName { get; set; }

        public string Source { get; set; }

        public string IssuerName { get; set; }

        public bool Validated { get; set; }

        public string Content { get; set; }

        public AttributeState State { get; set; }

        public bool IsOverriden { get; set; }
    }
}
