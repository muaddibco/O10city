using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model
{
    [Table("ServiceProviderRegistrations")]
    public class ServiceProviderRegistration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ServiceProviderRegistrationId { get; set; }

        public long AccountId { get; set; }

        [Required]
        public byte[] Commitment { get; set; }
    }
}
