using O10.Core;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Gateway.Common.Services.Results;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services
{
    [ServiceContract]
    public interface ITransactionsHandler : IDynamicPipe
    {
        TaskCompletionSource<ResultBase> SendPacket(PacketBase packetBase);
    }
}
