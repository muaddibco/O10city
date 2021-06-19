using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.ServiceProviders
{
    [Table("group_relations")]
    public class GroupRelation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long GroupRelationId { get; set; }
        public string GroupOwnerKey { get; set; }
        public string GroupName { get; set; }
        public string AssetId { get; set; }
        public string Issuer { get; set; }
    }
}
