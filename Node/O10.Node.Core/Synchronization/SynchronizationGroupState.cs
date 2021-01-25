using System.Collections.Concurrent;
using System.Reactive.Subjects;
using O10.Core.Architecture;
using O10.Core.Identity;
using O10.Core.States;

namespace O10.Node.Core.Synchronization
{

    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationGroupState : NeighborhoodStateBase, ISynchronizationGroupState
    {
        public const string NAME = nameof(ISynchronizationGroupState);

        private readonly ConcurrentDictionary<IKey, IKey> _participants;
        private readonly Subject<string> _subject = new Subject<string>();

        public SynchronizationGroupState()
        {
            _participants = new ConcurrentDictionary<IKey, IKey>();
        }

        public override string Name => NAME;

        public bool CheckParticipant(IKey key)
        {
            return _neighbors.Contains(key);
        }
    }
}
