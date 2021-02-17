using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("TransactionKeys")]
    public class TransactionKey
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionKeyId { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string Key { get; set; }
    }
}
