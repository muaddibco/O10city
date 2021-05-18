using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Core.Logging;
using O10.Transactions.Core.Ledgers;
using O10.Network.Interfaces;

namespace O10.Node.Core.Centralized
{
    public abstract class StorageHandlerBase<T> : ILedgerPacketsHandler where T : IPacketBase
    {
        private readonly IChainDataService _chainDataService;
        private readonly IRealTimeRegistryService _realTimeRegistryService;
		private readonly ILogger _logger;

		private ActionBlock<T> _storeBlock;
        private CancellationToken _cancellationToken;

        public StorageHandlerBase(IChainDataServicesManager chainDataServicesManager,
                                  IRealTimeRegistryService realTimeRegistryService,
                                  ILoggerService loggerService)
        {
            _chainDataService = chainDataServicesManager.GetChainDataService(LedgerType);
            _realTimeRegistryService = realTimeRegistryService;
			_logger = loggerService.GetLogger(GetType().Name);
		}

        public abstract string Name { get; }

        public abstract LedgerType LedgerType { get; }

        public void Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _storeBlock = new ActionBlock<T>(StoreBlock, new ExecutionDataflowBlockOptions { BoundedCapacity = int.MaxValue,  CancellationToken = _cancellationToken, MaxDegreeOfParallelism = 1 });
        }

        public void ProcessPacket(IPacketBase packet)
        {
            _storeBlock.Post((T)packet);
        }

        private void StoreBlock(T packet)
        {
			_logger.LogIfDebug(() => $"Storing packet {packet.GetType().Name}");

			try
			{
                _realTimeRegistryService.PostTransaction(_chainDataService.Add(packet));
            }
            catch (Exception ex)
			{
				_logger.Error($"Storing packet {packet.GetType().Name} failed", ex);
			}
        }
    }
}
