using O10.Core;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Transactions.Core.Serializers.RawPackets
{
    [ServiceContract]
    public interface IRawPacketProvidersFactory : IFactory<IRawPacketProvider>
    {
        IRawPacketProvider Create(IPacket blockBase);
		IRawPacketProvider Create(byte[] content);
    }
}
