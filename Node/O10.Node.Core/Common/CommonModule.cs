using O10.Transactions.Core.Interfaces;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Core.Modularity;

namespace O10.Node.Core.Common
{
    [RegisterExtension(typeof(IModule), Lifetime = LifetimeManagement.Singleton)]
    public class CommonModule : ModuleBase
    {
        public const string NAME = nameof(CommonModule);
        private readonly IBlocksHandlersRegistry _blocksHandlersFactory;

        public CommonModule(ILoggerService loggerService, IBlocksHandlersRegistry blocksHandlersFactory) : base(loggerService)
        {
            _blocksHandlersFactory = blocksHandlersFactory;
        }

        public override string Name => NAME;

        protected override void Start()
        {
        }

        protected override void InitializeInner()
        {
            IBlocksHandler blocksHandler = _blocksHandlersFactory.GetInstance(SynchronizationReceivingHandler.NAME);
            _blocksHandlersFactory.RegisterInstance(blocksHandler);
            blocksHandler.Initialize(_cancellationToken);
        }
    }
}
