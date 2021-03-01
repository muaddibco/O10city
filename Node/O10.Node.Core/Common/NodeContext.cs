using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks.Dataflow;
using O10.Core.Architecture;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.States;

namespace O10.Node.Core.Common
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class NodeContext : INodeContext
    {
        public const string NAME = nameof(INodeContext);

        private readonly Subject<string> _subject = new Subject<string>();
        private bool _disposedValue;

        public NodeContext()
        {
            //SyncGroupParticipants = new List<SynchronizationGroupParticipant>();
        }

        public string Name => NAME;

        public IKey AccountKey { get; private set; }

        //public List<SynchronizationGroupParticipant> SyncGroupParticipants { get; private set; }

        public ISigningService SigningService { get; private set; }

        public void Initialize(ISigningService signingService)
        {
            SigningService = signingService;
            AccountKey = SigningService.PublicKeys[0];
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
        // ~NodeContext()
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
