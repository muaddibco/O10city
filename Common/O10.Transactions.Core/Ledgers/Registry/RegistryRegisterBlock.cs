using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Registry
{
    public class RegistryRegisterBlock : RegistryBlockBase
    {
        public override ushort PacketType => PacketTypes.Registry_Register;

        public override ushort Version => 1;

        public LedgerType ReferencedLedgerType { get; set; }

        public ushort ReferencedBlockType { get; set; }

        public byte[] ReferencedBodyHash { get; set; }

        public byte[] ReferencedTarget { get; set; }

		public byte[] ReferencedTransactionKey { get; set; }
	}
}
