using System.Threading.Tasks;
using O10.Client.Common.Communication.Notifications;
using O10.Core.Models;

namespace O10.Client.Common.Communication
{
    public class PacketWrapper
    {
        public PacketWrapper(PacketBase packet)
        {
            Packet = packet;
            TaskCompletion = new TaskCompletionSource<NotificationBase>();
        }

        public TaskCompletionSource<NotificationBase> TaskCompletion { get; }

        public PacketBase Packet { get; }
    }
}
