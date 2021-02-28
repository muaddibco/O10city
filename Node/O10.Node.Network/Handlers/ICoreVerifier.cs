using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers;

namespace O10.Network.Handlers
{
    [ExtensionPoint]
    internal interface ICoreVerifier
    {
        bool VerifyBlock(IPacketBase packet);
    }
}
