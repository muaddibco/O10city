using O10.Core.Notifications;
using System.Threading.Tasks;

namespace O10.Core.Models
{
    public class TaskCompletionWrapper<T>
    {
        public TaskCompletionWrapper(T state, object? argument = null)
        {
            State = state;
            Argument = argument;
            TaskCompletion = new TaskCompletionSource<NotificationBase>(state);
        }

        public TaskCompletionSource<NotificationBase> TaskCompletion { get; }

        public T State { get; }

        public object? Argument { get; set; }

        public bool TryToComplete(NotificationBase notification)
        {
            return TaskCompletion.TrySetResult(notification);
        }
    }
}
