using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using O10.Client.DataLayer.Enums;

namespace O10.Client.DataLayer.Model
{
    [Table("sp_identity_validations")]
	public class SpIdenitityValidation
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long SpIdenitityValidationId { get; set; }

		[Required]
		public long AccountId { get; set; }

		public string SchemeName { get; set; }

		public ValidationType ValidationType { get; set; }

		public ushort? NumericCriterion { get; set; }

		public byte[] GroupIdCriterion { get; set; }
	}
}
