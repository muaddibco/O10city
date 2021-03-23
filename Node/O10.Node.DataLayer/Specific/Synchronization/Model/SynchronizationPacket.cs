using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Node.DataLayer.Specific.Synchronization.Model
{
	[Table("SynchronizationPackets")]
    public class SynchronizationPacket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long SynchronizationPacketId { get; set; }

        public DateTime ReceiveTime { get; set; }

        public DateTime MedianTime { get; set; }

        public string Content { get; set; }
    }
}
