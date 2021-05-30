using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("identity")]
    public class Identity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long IdentityId { get; set; }

        public long AccountId { get; set; }

        public string Description { get; set; }

        public virtual ICollection<IdentityAttribute> Attributes { get; set; }
    }
}
