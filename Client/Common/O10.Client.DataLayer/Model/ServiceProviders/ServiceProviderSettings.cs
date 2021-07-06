using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using O10.Client.DataLayer.Enums;

namespace O10.Client.DataLayer.Model.ServiceProviders
{
    [Table("ServiceProviderSettings")]
    public class ServiceProviderSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ServiceProviderSettingsId { get; set; }

        public long AccountId { get; set; }

        public ServiceProviderType ServiceProviderType { get; set; }
    }
}
