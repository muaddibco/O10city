using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Node.Network.Topology;
using O10.Node.Core.DataLayer;

namespace O10.Node.Core.Common
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
    public class CommonTopologyInitializer : InitializerBase
    {
        private readonly INodesDataService _nodesDataService;
		private readonly INodesResolutionService _nodesResolutionService;
		private readonly ILogger _logger;

        public CommonTopologyInitializer(INodesDataService nodesDataService, INodesResolutionService nodesResolutionService, ILoggerService loggerService)
        {
            _nodesDataService = nodesDataService;
			_nodesResolutionService = nodesResolutionService;
			_logger = loggerService.GetLogger(nameof(CommonTopologyInitializer));
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.AboveNormal;

        protected override async Task InitializeInner(CancellationToken cancellationToken)
        {
            foreach (var node in await _nodesDataService.Get(null, cancellationToken))
            {
				_nodesResolutionService.AddNode(new NodeAddress(node.Key, node.IPAddress), node.NodeRole);
			}
        }
    }
}
