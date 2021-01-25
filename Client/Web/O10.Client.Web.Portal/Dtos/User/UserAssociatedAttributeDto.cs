namespace O10.Client.Web.Portal.Dtos.User
{
    public class UserAssociatedAttributeDto
    {
        public string SchemeName { get; set; }
        public string Alias { get; set; }
        public string Content { get; set; }
        public string ValueType { get; set; }

        public long AttributeId { get; set; }
    }
}
