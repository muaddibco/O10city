using O10.Core;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.Notifications;
using System.Threading.Tasks;

namespace O10.Gateway.Common.Services
{
    [ServiceContract]
    public interface ITransactionsHandler : IDynamicPipe
    {
        TaskCompletionSource<NotificationBase> SendPacket(PacketBase packetBase);
    }
}
