using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using O10.Client.DataLayer.Enums;

namespace O10.Client.DataLayer.Model
{
    [Table("IdentityAttributes")]
    public class IdentityAttribute
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AttributeId { get; set; }

        public Identity? Identity { get; set; }

        [Required]
        public string AttributeName { get; set; }

        [Required]
        public string Content { get; set; }

        public ClaimSubject Subject { get; set; }

        public string? Commitment { get; set; }

    }
}
