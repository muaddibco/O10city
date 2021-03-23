using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.Synchronization.Model
{
	[Table("AggregatedRegistrationsTransactions")]
    public class AggregatedRegistrationsTransaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long AggregatedRegistrationsTransactionId { get; set; }

        public long SyncBlockHeight { get; set; }

        public string Content { get; set; }

		public string FullBlockHashes { get; set; }
	}
}
