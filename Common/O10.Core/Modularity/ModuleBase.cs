using System;
using System.Threading;
using System.Threading.Tasks;
using O10.Core.Logging;

namespace O10.Core.Modularity
{
    public abstract class ModuleBase : IModule
    {
        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1);
        protected readonly ILogger _log;
        protected CancellationToken _cancellationToken;

        public ModuleBase(ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
        }

        public abstract string Name { get; }

        public bool IsInitialized { get; private set; }

        public async Task Initialize(CancellationToken ct)
        {
            if (IsInitialized)
                return;

            _cancellationToken = ct;

            await _sync.WaitAsync();

            try
            {
                if (IsInitialized)
                    return;

                await InitializeInner();

                IsInitialized = true;

            }
            finally
            {
                _sync.Release();
            }
        }

        protected abstract Task InitializeInner();
        protected abstract void Start();

        public void StartModule()
        {
            try
            {
                Start();
            }
            catch (Exception ex)
            {
                _log.Error("Failed to start", ex);
                throw;
            }
        }
    }
}
