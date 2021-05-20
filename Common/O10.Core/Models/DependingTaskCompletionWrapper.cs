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

            TaskCompletion.Task.ContinueWith(t => 
            { 
                if(t.Exception != null)
                {
                    DependingTaskCompletion.TaskCompletion.SetException(t.Exception.InnerException);
                }
                else
                {
                    DependingTaskCompletion.TaskCompletion.SetResult(t.Result);
                }
            }, TaskScheduler.Current);
        }

        public TaskCompletionWrapper<TDepend> DependingTaskCompletion { get; }

        public async Task<IEnumerable<NotificationBase>> WaitForCompletion()
        {
            return await Task.WhenAll(new Task<NotificationBase>[] { TaskCompletion.Task, DependingTaskCompletion.TaskCompletion.Task }).ConfigureAwait(false);
        }
    }
}
