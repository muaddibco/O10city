using O10.Core.Architecture;
using O10.Core.States;

namespace O10.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IState), Lifetime = LifetimeManagement.Singleton)]
    public class SyncRegistryNeighborhoodState : NeighborhoodStateBase, ISyncRegistryNeighborhoodState
    {
        public override string Name => nameof(ISyncRegistryNeighborhoodState);
    }
}
