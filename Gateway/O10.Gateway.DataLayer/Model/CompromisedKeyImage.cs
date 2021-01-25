using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("CompromisedKeyImages")]
    public class CompromisedKeyImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CompromisedKeyImageId { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string KeyImage { get; set; }
    }
}
