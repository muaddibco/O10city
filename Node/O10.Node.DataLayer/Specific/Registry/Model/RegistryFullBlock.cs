using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.Registry.Model
{
    [Table("RegistryFullBlocks")]
    public class RegistryFullBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long RegistryFullBlockId { get; set; }

        public ulong SyncBlockHeight { get; set; }

        public ulong Round { get; set; }

        public int TransactionsCount { get; set; }

        public string Content { get; set; }

		[Column(TypeName = "varbinary(64)")]
        public string Hash { get; set; }

		public string HashString { get; set; }
	}
}
