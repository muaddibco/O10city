using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.States;
using O10.Network.Topology;
using O10.Node.Core.DataLayer;

namespace O10.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
    public class SynchronizationTopologyInitializer : InitializerBase
    {
        private readonly ISynchronizationGroupState _synchronizationGroupState;
        private readonly ISyncRegistryNeighborhoodState _syncRegistryNeighborhoodState;
        private readonly INodesDataService _nodesDataService;
        private readonly ILogger _logger;

        public SynchronizationTopologyInitializer(IStatesRepository statesRepository, INodesDataService nodesDataService, ILoggerService loggerService)
        {
            _synchronizationGroupState = statesRepository.GetInstance<ISynchronizationGroupState>();
            _syncRegistryNeighborhoodState = statesRepository.GetInstance<ISyncRegistryNeighborhoodState>();
            _nodesDataService = nodesDataService;
            _logger = loggerService.GetLogger(nameof(SynchronizationTopologyInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override async Task InitializeInner(CancellationToken cancellationToken)
        {
            foreach (var node in (await _nodesDataService.Get(null, cancellationToken)).Where(n => n.NodeRole == NodeRole.SynchronizationLayer))
            {
                _synchronizationGroupState.AddNeighbor(node.Key);
            }

            foreach (var node in (await _nodesDataService.Get(null, cancellationToken)).Where(n => n.NodeRole == NodeRole.TransactionsRegistrationLayer))
            {
                _syncRegistryNeighborhoodState.AddNeighbor(node.Key);
            }
        }
    }
}
