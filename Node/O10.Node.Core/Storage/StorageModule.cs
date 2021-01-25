using O10.Transactions.Core.Interfaces;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.Modularity;

namespace O10.Node.Core.Storage
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class StorageModule : ModuleBase
    {
        public const string NAME = nameof(StorageModule);
        private readonly IBlocksHandlersRegistry _blocksHandlersFactory;

        public StorageModule(ILoggerService loggerService, IBlocksHandlersRegistry blocksHandlersFactory) : base(loggerService)
        {
            _blocksHandlersFactory = blocksHandlersFactory;
        }

        public override string Name => NAME;

        protected override void Start()
        {
            
        }

        protected override void InitializeInner()
        {
            IBlocksHandler transactionalStorageBlocksHandler = _blocksHandlersFactory.GetInstance(TransactionalStorageHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(transactionalStorageBlocksHandler);
            transactionalStorageBlocksHandler.Initialize(_cancellationToken);
            IBlocksHandler StealthStorageBlocksHandler = _blocksHandlersFactory.GetInstance(StealthStorageHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(StealthStorageBlocksHandler);
            StealthStorageBlocksHandler.Initialize(_cancellationToken);
        }
    }
}
