namespace O10.Client.Common.Dtos
{
    public class AttributeDefinitionDTO
    {
        public long SchemeId { get; set; }
        public string AttributeName { get; set; }
        public string SchemeName { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsRoot { get; set; }
    }
}
