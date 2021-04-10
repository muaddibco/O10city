using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("StateTransactions")]
    public class StateTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long StateTransactionId { get; set; }

        public long WitnessId { get; set; }

        public ushort TransactionType { get; set; }

        [Required]
        public Address Source { get; set; }

        public Address? Target { get; set; }

        [Required]
        public string Content { get; set; }

        public TransactionHash? Hash { get; set; }

        #region Transition from State to Stealth based accounts

        public TransactionKey? TransactionKey { get; set; }

        public StealthOutput? Output { get; set; }

        #endregion Transition from State to Stealth based accounts
    }
}
