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

        public byte[] Content { get; set; }

        public UtxoTransactionKey TransactionKey { get; set; }

        public UtxoKeyImage KeyImage { get; set; }

        public UtxoOutput Output { get; set; }

        public PacketHash ThisBlockHash { get; set; }
    }
}
