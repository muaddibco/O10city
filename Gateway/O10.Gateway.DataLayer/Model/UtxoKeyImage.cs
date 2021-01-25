using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("UtxoKeyImages")]
    public class UtxoKeyImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UtxoKeyImageId { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string KeyImage { get; set; }
    }
}
