using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("SyncBlocks")]
    public class SyncBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long SyncBlockId { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string Hash { get; set; }
    }
}
