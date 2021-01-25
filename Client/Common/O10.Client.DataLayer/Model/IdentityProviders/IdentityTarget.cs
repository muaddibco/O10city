using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("IdentityTargets")]
    public class IdentityTarget
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdentityTargetId { get; set; }
        public long IdentityId { get; set; }
        public string PublicSpendKey { get; set; }
        public string PublicViewKey { get; set; }
    }
}
