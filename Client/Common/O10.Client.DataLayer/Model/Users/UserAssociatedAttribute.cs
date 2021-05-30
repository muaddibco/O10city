using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("user_associated_attribute")]
    public class UserAssociatedAttribute
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserAssociatedAttributeId { get; set; }

        public long AccountId { get; set; }

        public string AttributeSchemeName { get; set; }

        public string Content { get; set; }

        [Required]
        public string Source { get; set; }

        public DateTime? CreationTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }

        public byte[] RootAssetId { get; set; }
    }
}
