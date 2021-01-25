using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.O10Id.Model
{
	[Table("O10Transactions")]
    public class O10Transaction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long O10TransactionId { get; set; }

        public O10TransactionIdentity Identity { get; set; }

        public O10TransactionHashKey HashKey { get; set; }

        public long SyncBlockHeight { get; set; }

        public long BlockHeight { get; set; }

        public ushort BlockType { get; set; }

        public byte[] BlockContent { get; set; }
    }
}
