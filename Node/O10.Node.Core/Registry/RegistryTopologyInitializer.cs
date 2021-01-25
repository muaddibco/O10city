using System.Linq;
using System.Threading;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.States;
using O10.Network.Topology;
using O10.Node.Core.DataLayer;

namespace O10.Node.Core.Registry
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryTopologyInitializer : InitializerBase
    {
        private readonly IRegistryGroupState _registryGroupState;
        private readonly INodesDataService _nodesDataService;
        private readonly ILogger _logger;

        public RegistryTopologyInitializer(IStatesRepository statesRepository, INodesDataService nodesDataService, ILoggerService loggerService)
        {
            _registryGroupState = statesRepository.GetInstance<IRegistryGroupState>();
            _nodesDataService = nodesDataService;
            _logger = loggerService.GetLogger(nameof(RegistryTopologyInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            foreach (var node in _nodesDataService.Get(null).Where(n => n.NodeRole == NodeRole.TransactionsRegistrationLayer))
            {
                _registryGroupState.AddNeighbor(node.Key);
            }

            var syncNodes = _nodesDataService.Get(null).Where(n => n.NodeRole == NodeRole.SynchronizationLayer);
            _registryGroupState.SyncLayerNode = syncNodes.FirstOrDefault()?.Key;
        }
    }
}
