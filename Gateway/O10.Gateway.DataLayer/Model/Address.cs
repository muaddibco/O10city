using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
	[Table("Addresses")]
    public class Address
    {
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.Identity)]
        public long AddressId { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string Key { get; set; }
    }
}
