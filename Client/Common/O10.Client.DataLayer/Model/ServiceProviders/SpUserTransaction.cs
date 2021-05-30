using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Client.DataLayer.Model.ServiceProviders
{
    [Table("sp_user_transactions")]
    public class SpUserTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SpUserTransactionId { get; set; }

        public long AccountId { get; set; }

        public long ServiceProviderRegistrationId { get; set; }

        public string TransactionId { get; set; }

        public string TransactionDescription { get; set; }

        public bool IsProcessed { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsCompromised { get; set; }
    }
}
