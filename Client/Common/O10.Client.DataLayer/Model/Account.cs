using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using O10.Client.DataLayer.Enums;

namespace O10.Client.DataLayer.Model
{
    [Table("accounts")]
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountId { get; set; }

        public byte[] SecretViewKey { get; set; }
        public byte[] PublicViewKey { get; set; }
        public byte[] SecretSpendKey { get; set; }
        public byte[] PublicSpendKey { get; set; }
        [EnumDataType(typeof(AccountType))]
        public AccountType AccountType { get; set; }
        public string AccountInfo { get; set; }
        public bool IsCompromised { get; set; }

        public long LastAggregatedRegistrations { get; set; }

        public bool IsPrivate { get; set; }
    }
}
