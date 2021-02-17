using O10.Core.Architecture;
using O10.Transactions.Core.Enums;
using O10.Core.Models;

namespace O10.Network.Handlers
{
    [ExtensionPoint]
    public interface IPacketVerifier
    {
        LedgerType PacketType { get; }

        bool ValidatePacket(PacketBase block);
    }
}
