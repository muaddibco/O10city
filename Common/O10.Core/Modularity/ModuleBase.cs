using System;
using System.Threading;
using O10.Core.Logging;

namespace O10.Core.Modularity
{
    public abstract class ModuleBase : IModule
    {
        private readonly object _sync = new object();
        protected readonly ILogger _log;
        protected CancellationToken _cancellationToken;

        public ModuleBase(ILoggerService loggerService)
        {
            _log = loggerService.GetLogger(GetType().Name);
        }

        public abstract string Name { get; }

        public bool IsInitialized { get; private set; }

        public void Initialize(CancellationToken ct)
        {
            if (IsInitialized)
                return;

            _cancellationToken = ct;

            lock (_sync)
            {
                if (IsInitialized)
                    return;

                InitializeInner();

                IsInitialized = true;
            }
        }

        protected abstract void InitializeInner();
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
