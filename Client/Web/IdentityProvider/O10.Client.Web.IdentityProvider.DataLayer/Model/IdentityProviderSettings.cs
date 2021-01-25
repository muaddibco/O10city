using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.IdentityProvider.DataLayer.Model
{
	[Table("identity_provider_settings")]
	public class IdentityProviderSettings
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long IdentityProviderSettingsId { get; set; }

		public long AccountId { get; set; }
	}
}
