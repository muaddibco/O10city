using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Registry
{
    public class RegistryFullBlock : RegistryBlockBase
    {
        public override ushort PacketType => PacketTypes.Registry_FullBlock;

        public override ushort Version => 1;

        public RegistryRegisterBlock[] StateWitnesses { get; set; }
        public RegistryRegisterStealth[] StealthWitnesses { get; set; }
        public RegistryRegisterExBlock[] UniversalWitnesses { get; set; }

    public byte[] ShortBlockHash { get; set; }
    }
}
