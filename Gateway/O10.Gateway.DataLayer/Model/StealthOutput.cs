using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("StealthOutputs")]
    public class StealthOutput
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long StealthOutputId { get; set; }

		[Required]
		[Column(TypeName = "varbinary(64)")]
		public string DestinationKey { get; set; }

        [Required]
		[Column(TypeName = "varbinary(64)")]
		public string Commitment { get; set; }

        [Column(TypeName = "varbinary(64)")]
        public string? OriginatingCommitment { get; set; }

        public bool IsOverriden { get; set; }
    }
}
