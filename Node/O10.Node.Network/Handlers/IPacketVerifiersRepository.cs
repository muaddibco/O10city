using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Network.Handlers
{
    [ServiceContract]
    public interface IPacketVerifiersRepository : IRepository<IPacketVerifier, PacketType>
    {
    }
}
