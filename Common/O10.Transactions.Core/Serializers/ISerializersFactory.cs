using O10.Core;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Transactions.Core.Serializers
{
    [ServiceContract]
    public interface ISerializersFactory : IFactory<ISerializer, PacketBase>
    {
    }
}
