using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.Users
{
    [Table("user_registrations")]
    public class UserRegistration
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserRegistrationId { get; set; }

        public Account Account { get; set; }

        public string Commitment { get; set; }
        public string ServiceProviderInfo { get; set; }
        public string AssetId { get; set; }
        public string Issuer { get; set; }
    }
}
