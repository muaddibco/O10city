using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("UtxoOutputs")]
    public class UtxoOutput
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UtxoOutputId { get; set; }

		[Required]
		[Column(TypeName = "varbinary(64)")]
		public string DestinationKey { get; set; }

        [Required]
		[Column(TypeName = "varbinary(64)")]
		public string Commitment { get; set; }

        public byte[] OriginatingCommitment { get; set; }

        public bool IsOverriden { get; set; }
    }
}
