namespace O10.Client.Common.Entities
{
    public class ServiceProviderRelationGroups
    {
        public string PublicSpendKey { get; set; }
        public string PublicViewKey { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public RelationGroup[] RelationGroups { get; set; }
    }
}
