using O10.Transactions.Core.Ledgers.O10State;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Network.Interfaces;

namespace O10.Node.Core.Centralized
{
    [RegisterExtension(typeof(ILedgerPacketsHandler), Lifetime = LifetimeManagement.Scoped)]
    public class O10StateStorageHandler : StorageHandlerBase<O10StatePacket>
    {
        public const string NAME = "O10StateStorage";

        public O10StateStorageHandler(IChainDataServicesRepository chainDataServicesManager,
                                           IRealTimeRegistryService realTimeRegistryService,
                                           ILoggerService loggerService)
            : base(chainDataServicesManager, realTimeRegistryService, loggerService)
        {
        }

        public override string Name => NAME;

        public override LedgerType LedgerType => LedgerType.O10State;
    }
}
