using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Node.DataLayer.DataServices
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Scoped)]
	public class DataServicesInitializer : InitializerBase
	{
		private readonly IChainDataServicesManager _chainDataServicesManager;
		private readonly ILogger _logger;

		public DataServicesInitializer(IChainDataServicesManager chainDataServicesManager, ILoggerService loggerService)
		{
			_chainDataServicesManager = chainDataServicesManager;
			_logger = loggerService.GetLogger(nameof(DataServicesInitializer));
		}

		public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Normal;

		protected override async Task InitializeInner(CancellationToken cancellationToken)
		{
			foreach (var chainDataService in _chainDataServicesManager.GetAll())
			{
				try
				{
					chainDataService.Initialize(cancellationToken);
				}
				catch (System.Exception ex)
				{
					_logger.Error($"Failure during initializing chain data service {chainDataService.LedgerType}", ex);
				}
			}
		}
	}
}
