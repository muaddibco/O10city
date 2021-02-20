using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.Stealth.Model
{
	[Table("KeyImages")]
    public class KeyImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long KeyImageId { get; set; }

		[Column(TypeName = "varbinary(64)")]
        public string Value { get; set; }

        public string ValueString { get; set; }
	}
}
