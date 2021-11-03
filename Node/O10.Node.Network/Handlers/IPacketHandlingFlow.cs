using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers;
using System.Threading.Tasks;

namespace O10.Network.Handlers
{
    [ServiceContract]
    public interface IPacketHandlingFlow
    {
        Task PostPacket(IPacketBase packet);
    }
}
