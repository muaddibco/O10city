using System;
using System.Threading;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Core.Logging;
using O10.Transactions.Core.Ledgers;
using O10.Network.Interfaces;
using System.Threading.Tasks;

namespace O10.Node.Core.Centralized
{
    public abstract class StorageHandlerBase<T> : ILedgerPacketsHandler where T : IPacketBase
    {
        private readonly IChainDataService _chainDataService;
        private readonly IRealTimeRegistryService _realTimeRegistryService;
		private readonly ILogger _logger;

        private CancellationToken _cancellationToken;

        public StorageHandlerBase(IChainDataServicesRepository chainDataServicesManager,
                                  IRealTimeRegistryService realTimeRegistryService,
                                  ILoggerService loggerService)
        {
            _chainDataService = chainDataServicesManager.GetInstance(LedgerType);
            _realTimeRegistryService = realTimeRegistryService;
			_logger = loggerService.GetLogger(GetType().Name);
		}

        public abstract string Name { get; }

        public abstract LedgerType LedgerType { get; }

        public async Task Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
        }

        public async Task ProcessPacket(IPacketBase packet)
        {
            _logger.Debug(() => $"Storing packet {packet.GetType().Name}");

            try
            {
                _realTimeRegistryService.PostTransaction(await _chainDataService.Add(packet));
            }
            catch (Exception ex)
            {
                _logger.Error($"Storing packet {packet.GetType().Name} failed", ex);
            }
        }
    }
}
