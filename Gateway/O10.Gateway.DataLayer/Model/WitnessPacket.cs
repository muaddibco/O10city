﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace O10.Gateway.DataLayer.Model
{
	[Table("WitnessPackets")]
    public class WitnessPacket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long WitnessPacketId { get; set; }

        public long SyncBlockHeight { get; set; }

        public long Round { get; set; }

        public long CombinedBlockHeight { get; set; }

        public ushort ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

        public PacketHash ReferencedBodyHash { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string ReferencedDestinationKey { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string ReferencedDestinationKey2 { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string ReferencedTransactionKey { get; set; }

		[Column(TypeName = "varbinary(64)")]
		public string ReferencedKeyImage { get; set; }
	}
}
