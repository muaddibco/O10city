using O10.Transactions.Core.Interfaces;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.Modularity;

namespace O10.Node.Core.Registry
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistrationModule : ModuleBase
    {
        public const string NAME = nameof(TransactionsRegistrationModule);
        private readonly IPacketsHandlersRegistry _blocksHandlersFactory;
        private readonly ITransactionsRegistryService _transactionsRegistryService;

        public TransactionsRegistrationModule(ILoggerService loggerService, IPacketsHandlersRegistry blocksHandlersFactory, ITransactionsRegistryService transactionsRegistryService) : base(loggerService)
        {
            _blocksHandlersFactory = blocksHandlersFactory;
            _transactionsRegistryService = transactionsRegistryService;
        }

        public override string Name => NAME;

        protected override void Start()
        {
            _transactionsRegistryService.Start();
        }

        protected override void InitializeInner()
        {
            IPacketsHandler blocksHandler = _blocksHandlersFactory.GetInstance(TransactionsRegistryHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(blocksHandler);
            blocksHandler.Initialize(_cancellationToken);

            _transactionsRegistryService.Initialize();
        }
    }
}
