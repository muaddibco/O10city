using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("UtxoKeyImages")]
    public class KeyImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long KeyImageId { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string Value { get; set; }
    }
}
