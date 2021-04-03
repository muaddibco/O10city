using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("StealthPackets")]
    public class StealthPacket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long StealthPacketId { get; set; }

        public long WitnessId { get; set; }

        public ushort BlockType { get; set; }

        public string Content { get; set; }

        public TransactionKey TransactionKey { get; set; }

        public KeyImage KeyImage { get; set; }

        public StealthOutput Output { get; set; }

        public PacketHash ThisBlockHash { get; set; }
    }
}
