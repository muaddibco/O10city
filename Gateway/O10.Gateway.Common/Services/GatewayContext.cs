using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.States;
using O10.Transactions.Core.DTOs;
using System;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace O10.Gateway.Common.Services
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class GatewayContext : IGatewayContext
    {
        public const string NAME = nameof(IGatewayContext);

        private readonly Subject<string> _subject = new Subject<string>();
        private readonly INetworkSynchronizer _networkSynchronizer;
        private bool _disposedValue;

        public GatewayContext(INetworkSynchronizer networkSynchronizer)
        {
            _networkSynchronizer = networkSynchronizer;
        }

        public IKey AccountKey { get; private set; }

        public ISigningService SigningService { get; private set; }

        public string Name => NAME;

        public void Initialize(ISigningService signingService)
        {
            SigningService = signingService;
            AccountKey = SigningService.PublicKeys[0];
        }

        // TODO: Need to modify so last packet info won't be taken every time from a Node
        public async Task<StatePacketInfo> GetLastPacketInfo()
        {
            return await _networkSynchronizer.GetLastPacketInfo(SigningService.PublicKeys.First()).ConfigureAwait(false);
        }

        public IDisposable SubscribeOnStateChange(ITargetBlock<string> targetBlock)
        {
            return _subject.Subscribe(targetBlock.AsObserver());
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _subject.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~GatewayContext()
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
