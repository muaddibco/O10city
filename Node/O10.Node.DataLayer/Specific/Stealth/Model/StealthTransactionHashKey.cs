using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.Stealth.Model
{
	[Table("StealthTransactionHashKeys")]
    public class StealthTransactionHashKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long StealthTransactionHashKeyId { get; set; }

        public ulong CombinedBlockHeight { get; set; }

        public ulong SyncBlockHeight { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string Hash { get; set; }
		
        public string HashString { get; set; }
    }
}
