using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;
using O10.Core.Logging;

namespace O10.Node.Core.Centralized
{
    [RegisterExtension(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
    public class O10StateStorageHandler : StorageHandlerBase<O10StatePacket>
    {
        public const string NAME = "O10StateStorage";

        public O10StateStorageHandler(IChainDataServicesManager chainDataServicesManager,
                                           IRealTimeRegistryService realTimeRegistryService,
                                           ILoggerService loggerService)
            : base(chainDataServicesManager, realTimeRegistryService, loggerService)
        {
        }

        public override string Name => NAME;

        public override LedgerType LedgerType => LedgerType.O10State;
    }
}
