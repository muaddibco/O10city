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

        public long RegistryHeight { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public byte[] Hash { get; set; }
    }
}
