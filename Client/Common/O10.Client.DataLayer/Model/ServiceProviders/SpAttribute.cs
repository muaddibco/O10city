using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("sp_attributes")]
    public class SpAttribute
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public long SpAttributeId { get; set; }

		[Required]
        public long AccountId { get; set; }

        public string AttributeSchemeName { get; set; }

        public string Content { get; set; }

        [Required]
        public byte[] AssetId { get; set; }

        [Required]
        public string Source { get; set; }

        [Required]
        public byte[] OriginalBlindingFactor { get; set; }

        [Required]
        public byte[] OriginalCommitment { get; set; }

        [Required]
        public byte[] IssuingCommitment { get; set; }
    }
}
