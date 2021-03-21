using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.Stealth.Model
{
	[Table("StealthTransactions")]
    public class StealthTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long StealthTransactionId { get; set; }

        public StealthTransactionHashKey HashKey { get; set; }

        public long RegistryHeight { get; set; }

        public KeyImage KeyImage { get; set; }

        public ushort BlockType { get; set; }

		[Column(TypeName = "varbinary(64)")]
        public string DestinationKey { get; set; }

		public string Content { get; set; }
    }
}
