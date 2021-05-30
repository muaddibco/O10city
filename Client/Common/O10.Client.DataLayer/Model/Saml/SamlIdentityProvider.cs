using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("saml_identity_providers")]
    public class SamlIdentityProvider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SamlIdentityProviderId { get; set; }

        public string EntityId { get; set; }

        public string SecretViewKey { get; set; }
        public string PublicSpendKey { get; set; }
    }
}
