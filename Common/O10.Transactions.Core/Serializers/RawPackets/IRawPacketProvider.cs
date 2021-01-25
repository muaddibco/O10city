using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.Models;

namespace O10.Transactions.Core.Serializers.RawPackets
{
    [ExtensionPoint]
    public interface IRawPacketProvider : IPacketProvider, ITransactionKeyProvider
    {
        void Initialize(IPacket blockBase);
		void Initialize(byte[] content);
    }
}
