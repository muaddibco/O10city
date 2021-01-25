using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
    [Table("TransactionalPackets")]
    public class TransactionalPacket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionalPacketId { get; set; }

        public long WitnessId { get; set; }

        public long Height { get; set; }

        public ushort BlockType { get; set; }

        public Address Source { get; set; }

        public Address Target { get; set; }

        public byte[] GroupId { get; set; }

        public byte[] Content { get; set; }

        #region Transition Account based to UTXO based transaction

        public bool IsTransition { get; set; }

        public UtxoTransactionKey TransactionKey { get; set; }

        public UtxoOutput Output { get; set; }

        #endregion Transition Account based to UTXO based transaction

        public bool IsVerified { get; set; }

        public bool IsValid { get; set; }

        public PacketHash ThisBlockHash { get; set; }
    }
}
