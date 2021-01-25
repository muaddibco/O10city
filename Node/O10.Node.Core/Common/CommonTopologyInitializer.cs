using System.Threading;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.Logging;
using O10.Core.States;
using O10.Network.Topology;
using O10.Node.Core.DataLayer;

namespace O10.Node.Core.Common
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class CommonTopologyInitializer : InitializerBase
    {
        private readonly INeighborhoodState _neighborhoodState;
        private readonly INodesDataService _nodesDataService;
		private readonly INodesResolutionService _nodesResolutionService;
		private readonly ILogger _logger;

        public CommonTopologyInitializer(IStatesRepository statesRepository, INodesDataService nodesDataService, INodesResolutionService nodesResolutionService, ILoggerService loggerService)
        {
            _neighborhoodState = statesRepository.GetInstance<INeighborhoodState>();
            _nodesDataService = nodesDataService;
			_nodesResolutionService = nodesResolutionService;
			_logger = loggerService.GetLogger(nameof(CommonTopologyInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.AboveNormal;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            foreach (var node in _nodesDataService.Get(null))
            {
                _neighborhoodState.AddNeighbor(node.Key);
				_nodesResolutionService.AddNode(new NodeAddress(node.Key, node.IPAddress), node.NodeRole);
			}
        }
    }
}
