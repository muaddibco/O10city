using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("TransactionHashes")]
    public class TransactionHash
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionHashId { get; set; }

        public long AggregatedTransactionsHeight { get; set; }

        [Required]
        [Column(TypeName = "varbinary(64)")]
		public string Hash { get; set; }

        public string HashString { get; set; }
    }
}
