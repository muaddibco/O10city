using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using O10.Client.DataLayer.Enums;

namespace O10.Client.DataLayer.Model.ServiceProviders
{
    [Table("service_provider_settings")]
    public class ServiceProviderSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ServiceProviderSettingsId { get; set; }

        public long AccountId { get; set; }

        public ServiceProviderType ServiceProviderType { get; set; }
    }
}
