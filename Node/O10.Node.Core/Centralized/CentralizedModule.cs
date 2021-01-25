using O10.Transactions.Core.Interfaces;
using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Modularity;

namespace O10.Node.Core.Centralized
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class CentralizedModule : ModuleBase
    {
        private readonly ILoggerService _loggerService;
        private readonly IBlocksHandlersRegistry _blocksHandlersRegistry;
		private readonly INotificationsService _notificationsService;
		private readonly IRealTimeRegistryService _realTimeRegistryService;
        private readonly IChainDataServicesManager _chainDataServicesManager;
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly IHashCalculationsRepository _hashCalculationsRepository;


        public CentralizedModule(ILoggerService loggerService,
                                 IBlocksHandlersRegistry blocksHandlersFactory,
                                 INotificationsService notificationsService,
                                 IRealTimeRegistryService realTimeRegistryService,
                                 IChainDataServicesManager chainDataServicesManager,
                                 IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                 IHashCalculationsRepository hashCalculationsRepository) : base(loggerService)
        {
            _loggerService = loggerService;
            _blocksHandlersRegistry = blocksHandlersFactory;
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

        protected override void InitializeInner()
        {
            IBlocksHandler blocksHandler = _blocksHandlersRegistry.GetInstance(TransactionsRegistryCentralizedHandler.NAME);
            _blocksHandlersRegistry.RegisterInstance(blocksHandler);
            blocksHandler.Initialize(_cancellationToken);

			blocksHandler = _blocksHandlersRegistry.GetInstance(TransactionalStorageHandler.NAME);
			_blocksHandlersRegistry.RegisterInstance(blocksHandler);
			blocksHandler.Initialize(_cancellationToken);

			blocksHandler = _blocksHandlersRegistry.GetInstance(StealthStorageHandler.NAME);
			_blocksHandlersRegistry.RegisterInstance(blocksHandler);
			blocksHandler.Initialize(_cancellationToken);
		}
	}
}
