using O10.Core.Notifications;
using System.Threading.Tasks;

namespace O10.Core.Models
{
    public class TaskCompletionWrapper<T>
    {
        public TaskCompletionWrapper(T state)
        {
            State = state;
            TaskCompletion = new TaskCompletionSource<NotificationBase>(state);
        }

        public TaskCompletionSource<NotificationBase> TaskCompletion { get; }

        public T State { get; }
    }
}
