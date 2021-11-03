using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Core.Architecture;
using O10.Core.Logging;
using O10.Network.Interfaces;

namespace O10.Node.Core.Centralized
{
    [RegisterExtension(typeof(ILedgerPacketsHandler), Lifetime = LifetimeManagement.Scoped)]
    public class StealthStorageHandler : StorageHandlerBase<StealthPacket>
    {
        public const string NAME = "StealthStorage";

        public StealthStorageHandler(IChainDataServicesRepository chainDataServicesManager,
                                     IRealTimeRegistryService realTimeRegistryService,
                                     ILoggerService loggerService)
			: base(chainDataServicesManager, realTimeRegistryService, loggerService)
        {
        }

        public override string Name => NAME;

        public override LedgerType LedgerType => LedgerType.Stealth;
    }
}
