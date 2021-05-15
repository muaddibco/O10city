using O10.Core.Architecture;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Core
{
    public abstract class InitializerBase : IInitializer, IDisposable
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _disposedValue;

        public bool Initialized { get; private set; }

        public abstract ExtensionOrderPriorities Priority { get; }

        public async Task Initialize(CancellationToken cancellationToken)
        {
            if(Initialized)
            {
                return;
            }

            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            if (Initialized)
            {
                return;
            }

            try
            {

                await InitializeInner(cancellationToken).ConfigureAwait(false);

                Initialized = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        protected abstract Task InitializeInner(CancellationToken cancellationToken);

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _semaphore.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~InitializerBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
