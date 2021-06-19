using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.ServiceProviders
{
    [Table("RelationGroups")]
    public class RelationGroup
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RelationGroupId { get; set; }

        public Account Account { get; set; }

        public string GroupName { get; set; }
    }
}
