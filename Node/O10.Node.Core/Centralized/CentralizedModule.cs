using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Modularity;
using O10.Network.Interfaces;
using System.Threading.Tasks;

namespace O10.Node.Core.Centralized
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Scoped)]
    public class CentralizedModule : ModuleBase
    {
        private readonly ILoggerService _loggerService;
        private readonly IPacketsHandlersRegistry _packetsHandlersRegistry;
		private readonly INotificationsService _notificationsService;
		private readonly IRealTimeRegistryService _realTimeRegistryService;
        private readonly IChainDataServicesRepository _chainDataServicesManager;
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly IHashCalculationsRepository _hashCalculationsRepository;


        public CentralizedModule(ILoggerService loggerService,
                                 IPacketsHandlersRegistry packetsHandlersRegistry,
                                 INotificationsService notificationsService,
                                 IRealTimeRegistryService realTimeRegistryService,
                                 IChainDataServicesRepository chainDataServicesManager,
                                 IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                 IHashCalculationsRepository hashCalculationsRepository) : base(loggerService)
        {
            _loggerService = loggerService;
            _packetsHandlersRegistry = packetsHandlersRegistry;
			_notificationsService = notificationsService;
			_realTimeRegistryService = realTimeRegistryService;
            _chainDataServicesManager = chainDataServicesManager;
            _identityKeyProvidersRegistry = identityKeyProvidersRegistry;
            _hashCalculationsRepository = hashCalculationsRepository;
        }

        public override string Name => nameof(CentralizedModule);

        protected override void Start()
        {
            _notificationsService.Initialize(_cancellationToken);

            _log.Info($"{nameof(CentralizedModule)} started");
        }

        protected override async Task InitializeInner()
        {
            ILedgerPacketsHandler blocksHandler = _packetsHandlersRegistry.GetInstance(TransactionsRegistryCentralizedHandler.NAME);
            _packetsHandlersRegistry.RegisterInstance(blocksHandler);
            blocksHandler.Initialize(_cancellationToken);

			blocksHandler = _packetsHandlersRegistry.GetInstance(O10StateStorageHandler.NAME);
			_packetsHandlersRegistry.RegisterInstance(blocksHandler);
			blocksHandler.Initialize(_cancellationToken);

			blocksHandler = _packetsHandlersRegistry.GetInstance(StealthStorageHandler.NAME);
			_packetsHandlersRegistry.RegisterInstance(blocksHandler);
			blocksHandler.Initialize(_cancellationToken);
		}
	}
}
