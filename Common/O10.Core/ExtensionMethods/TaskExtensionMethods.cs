using System;
using System.Threading.Tasks;

namespace O10.Core.ExtensionMethods
{
    public static class TaskExtensionMethods
    {
        public static async Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)).ConfigureAwait(false))
                await task.ConfigureAwait(false);
            else
                throw new TimeoutException();
        }

        public static async Task<T> TimeoutAfter<T>(this Task<T> task, int millisecondsTimeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(millisecondsTimeout)).ConfigureAwait(false))
                return await task.ConfigureAwait(false);
            else
                throw new TimeoutException();
        }
    }
}
