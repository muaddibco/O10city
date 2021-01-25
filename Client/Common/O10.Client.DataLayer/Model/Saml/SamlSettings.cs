using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("saml_settings")]
	public class SamlSettings
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long SamlSettingsId { get; set; }

		public long DefaultSamlIdpId { get; set; }

		public long DefaultSamlIdpAccountId { get; set; }
	}
}
