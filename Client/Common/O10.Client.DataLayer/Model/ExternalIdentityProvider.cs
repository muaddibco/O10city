using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("external_identity_providers")]
    public class ExternalIdentityProvider
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ExternalIdentityProviderId { get; set; }

        public string Name { get; set; }

        public string Alias { get; set; }

        public string Description { get; set; }

        public long AccountId { get; set; }
    }
}
