using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.Synchronization.Model
{
	[Table("SynchronizationBlocks")]
    public class SynchronizationBlock
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long SynchronizationBlockId { get; set; }

        public DateTime ReceiveTime { get; set; }

        public DateTime MedianTime { get; set; }

        public byte[] BlockContent { get; set; }
    }
}
