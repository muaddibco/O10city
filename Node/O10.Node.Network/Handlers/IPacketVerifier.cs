using O10.Core.Architecture;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;

namespace O10.Network.Handlers
{
    [ExtensionPoint]
    public interface IPacketVerifier
    {
        LedgerType LedgerType { get; }

        bool ValidatePacket(IPacketBase block);
    }
}
