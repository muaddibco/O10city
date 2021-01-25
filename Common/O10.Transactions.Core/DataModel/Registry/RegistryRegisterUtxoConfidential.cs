using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel.Registry
{
    public class RegistryRegisterStealth : StealthBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Registry;

        public override ushort Version => 1;

        public override ushort BlockType => ActionTypes.Registry_RegisterStealth;

        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedBlockType { get; set; }

		public byte[] ReferencedBodyHash { get; set; }
    }
}
