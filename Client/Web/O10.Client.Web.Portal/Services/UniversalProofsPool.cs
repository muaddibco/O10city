using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Core.Architecture;

using O10.Core.Identity;
using O10.Core.Logging;

namespace O10.Client.Web.Portal.Services
{
    [RegisterDefaultImplementation(typeof(IUniversalProofsPool), Lifetime = LifetimeManagement.Singleton)]
    public class UniversalProofsPool : IUniversalProofsPool
    {
        private readonly ConcurrentDictionary<IKey, TaskCompletionSource<UniversalProofs>> _universalProofs = new ConcurrentDictionary<IKey, TaskCompletionSource<UniversalProofs>>(new KeyEqualityComparer());
        private readonly ConcurrentDictionary<IKey, Task> _universalProofsMonitor = new ConcurrentDictionary<IKey, Task>();

        private readonly ILogger _logger;

        public UniversalProofsPool(ILoggerService loggerService)
        {
            _logger = loggerService.GetLogger(nameof(UniversalProofsPool));
        }

        public TaskCompletionSource<UniversalProofs> Extract(IKey keyImage)
        {
            Contract.Requires(keyImage != null, nameof(keyImage));

            _logger.LogIfDebug(() => $"Extracting {nameof(UniversalProofs)} with {nameof(keyImage)}={keyImage}");

            TaskCompletionSource<UniversalProofs> taskCompletionSource = _universalProofs.GetOrAdd(keyImage, new TaskCompletionSource<UniversalProofs>());
            _universalProofsMonitor.AddOrUpdate(keyImage,
                Task.Delay(1000)
                .ContinueWith((t, o) =>
                {
                    _logger.LogIfDebug(() => $"Monitoring {nameof(UniversalProofs)} with {nameof(keyImage)}={o}");
                    if (_universalProofs.TryRemove((IKey)o, out TaskCompletionSource<UniversalProofs> task))
                    {
                        if (!task.Task.IsCompleted)
                        {
                            _logger.LogIfDebug(() => $"Timeout for {nameof(UniversalProofs)} with {nameof(keyImage)}={o}");
                            task.SetException(new TimeoutException());
                        }
                        else
                        {
                            _logger.LogIfDebug(() => $"Removed {nameof(UniversalProofs)} with {nameof(keyImage)}={o} and {nameof(task.Task.Result.SessionKey)}={task.Task.Result.SessionKey}");
                        }
                    }

                    _universalProofsMonitor.TryRemove((IKey)o, out _);
                }, keyImage, TaskScheduler.Current), (k, v) => v);

            return taskCompletionSource;
        }

        public void Store(UniversalProofs universalProofs)
        {
            Contract.Requires(universalProofs != null && universalProofs.KeyImage != null);

            _logger.LogIfDebug(() => $"Storing {nameof(universalProofs)} with {nameof(universalProofs.KeyImage)}={universalProofs.KeyImage} and {nameof(universalProofs.SessionKey)}={universalProofs.SessionKey}");

            TaskCompletionSource<UniversalProofs> taskCompletionSource = new TaskCompletionSource<UniversalProofs>();
            taskCompletionSource.SetResult(universalProofs);
            _universalProofs.AddOrUpdate(universalProofs.KeyImage, taskCompletionSource, (k, v) =>
            {
                if (!v.Task.IsCompleted)
                {
                    v.SetResult(universalProofs);
                }

                return v;
            });
        }
    }
}
