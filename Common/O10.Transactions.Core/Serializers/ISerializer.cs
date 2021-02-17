using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.Models;

namespace O10.Transactions.Core.Serializers
{
    [ExtensionPoint]
    public interface ISerializer : IPacketProvider, ITransactionKeyProvider
    {
        LedgerType PacketType { get; }

        ushort BlockType { get; }

        void Initialize(PacketBase blockBase);

        void SerializeBody();

        void SerializeFully();
    }
}
