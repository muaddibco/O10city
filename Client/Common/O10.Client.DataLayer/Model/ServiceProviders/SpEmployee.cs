using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
	[Table("sp_employees")]
	public class SpEmployee
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long SpEmployeeId { get; set; }

		public Account Account { get; set; }

		public string Description { get; set; }

		[Required]
		public string RootAttributeRaw { get; set; }

        public string RegistrationCommitment { get; set; }

		public SpEmployeeGroup SpEmployeeGroup { get; set; }
	}
}
