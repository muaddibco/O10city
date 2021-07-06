using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("UserGroupRelations")]
    public class UserGroupRelation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserGroupRelationId { get; set; }

        public Account? Account { get; set; }

        public string GroupOwnerName { get; set; }
        public string GroupOwnerKey { get; set; }
        public string GroupName { get; set; }
        public string AssetId { get; set; }
        public string Issuer { get; set; }
    }
}
