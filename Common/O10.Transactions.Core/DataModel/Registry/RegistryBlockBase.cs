using O10.Core.Models;

namespace O10.Transactions.Core.DataModel.Registry
{
    public abstract class RegistryBlockBase : SignedPacketBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Registry;
    }
}
