using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("StealthTransactions")]
    public class StealthTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long StealthTransactionId { get; set; }

        public long WitnessId { get; set; }

        public ushort TransactionType { get; set; }

        public string Content { get; set; }

        public TransactionKey TransactionKey { get; set; }

        public KeyImage KeyImage { get; set; }

        public StealthOutput Output { get; set; }

        public TransactionHash Hash { get; set; }
    }
}
