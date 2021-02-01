using O10.Core.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace O10.Core.Models
{
    public class DependingTaskCompletionWrapper<T, TDepend> : TaskCompletionWrapper<T>
    {
        public DependingTaskCompletionWrapper(T state, TaskCompletionWrapper<TDepend> taskCompletionDependant) : base(state)
        {
            DependingTaskCompletion = taskCompletionDependant;
            CompletionAll = new TaskCompletionSource<IEnumerable<NotificationBase>>();
            Task
                .WhenAll(new Task<NotificationBase>[] { TaskCompletion.Task, DependingTaskCompletion.TaskCompletion.Task })
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        CompletionAll.SetResult(t.Result);
                    }
                    else
                    {
                        CompletionAll.SetException(t.Exception.InnerException);
                    }
                }, TaskScheduler.Default);
        }

        public TaskCompletionWrapper<TDepend> DependingTaskCompletion { get; }

        public TaskCompletionSource<IEnumerable<NotificationBase>> CompletionAll { get; set; }
    }
}
