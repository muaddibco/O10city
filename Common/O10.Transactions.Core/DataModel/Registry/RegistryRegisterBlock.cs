using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel.Registry
{
    public class RegistryRegisterBlock : RegistryBlockBase
    {
        public override ushort BlockType => ActionTypes.Registry_Register;

        public override ushort Version => 1;

        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

        public byte[] ReferencedBodyHash { get; set; }

        public byte[] ReferencedTarget { get; set; }

		public byte[] ReferencedTransactionKey { get; set; }
	}
}
