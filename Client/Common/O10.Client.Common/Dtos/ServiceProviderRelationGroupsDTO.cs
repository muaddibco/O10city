namespace O10.Client.Common.Dtos
{
    public class ServiceProviderRelationGroupsDTO
    {
        public string PublicSpendKey { get; set; }
        public string PublicViewKey { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public RelationGroupDTO[] RelationGroups { get; set; }
    }
}
