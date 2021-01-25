using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("UtxoTransactionKeys")]
    public class UtxoTransactionKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UtxoTransactionKeyId { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string Key { get; set; }
    }
}
