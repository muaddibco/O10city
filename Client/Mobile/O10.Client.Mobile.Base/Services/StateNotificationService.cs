using System.Threading.Tasks.Dataflow;
using O10.Core.Architecture;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models.StateNotifications;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterDefaultImplementation(typeof(IStateNotificationService), Lifetime = LifetimeManagement.Singleton)]
    public class StateNotificationService : IStateNotificationService
    {
        public BroadcastBlock<StateNotificationBase> NotificationsPipe { get; } = new BroadcastBlock<StateNotificationBase>(n => (StateNotificationBase)n.Clone());
    }
}
