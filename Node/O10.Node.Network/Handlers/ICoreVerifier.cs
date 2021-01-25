using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Network.Handlers
{
    [ExtensionPoint]
    internal interface ICoreVerifier
    {
        bool VerifyBlock(PacketBase blockBase);
    }
}
