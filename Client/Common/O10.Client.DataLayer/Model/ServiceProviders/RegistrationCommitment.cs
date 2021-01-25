using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.ServiceProviders
{
	[Table("registration_commitments")]
	public class RegistrationCommitment
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long RegistrationCommitmentId { get; set; }
		public string Commitment { get; set; }
		public string ServiceProviderInfo { get; set; }
		public string AssetId { get; set; }
		public string Issuer { get; set; }
	}
}
