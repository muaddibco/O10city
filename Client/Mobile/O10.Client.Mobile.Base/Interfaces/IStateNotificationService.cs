using System.Threading.Tasks.Dataflow;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Models.StateNotifications;

namespace O10.Client.Mobile.Base.Interfaces
{
    [ServiceContract]
    public interface IStateNotificationService
    {
        BroadcastBlock<StateNotificationBase> NotificationsPipe { get; }
    }
}
