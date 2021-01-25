using System.Threading.Tasks;
using O10.Core.Models;

namespace O10.Client.Common.Communication
{
    public class PacketWrapper
    {
        public PacketWrapper(PacketBase packet)
        {
            Packet = packet;
            TaskCompletion = new TaskCompletionSource<bool>();
        }

        public TaskCompletionSource<bool> TaskCompletion { get; }

        public PacketBase Packet { get; }
    }
}
