using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Node.DataLayer.DataServices;
using O10.Core.Logging;
using O10.Transactions.Core.Ledgers;

namespace O10.Node.Core.Centralized
{
    public abstract class StorageHandlerBase<T> : IPacketsHandler where T : IPacketBase
    {
        private readonly IChainDataServicesManager _chainDataServicesManager;
		private readonly IRealTimeRegistryService _realTimeRegistryService;
		private readonly ILogger _logger;
		private ActionBlock<T> _storeBlock;
        private CancellationToken _cancellationToken;

        public StorageHandlerBase(IChainDataServicesManager chainDataServicesManager,
                                  IRealTimeRegistryService realTimeRegistryService,
                                  ILoggerService loggerService)
        {
            _chainDataServicesManager = chainDataServicesManager;
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

        public void ProcessBlock(IPacketBase packet)
        {
            _storeBlock.Post((T)packet);
			_realTimeRegistryService.PostTransaction(packet);
        }

        private void StoreBlock(T packet)
        {
			_logger.LogIfDebug(() => $"Storing packet {packet.GetType().Name}");

			try
			{
				IChainDataService chainDataService = _chainDataServicesManager.GetChainDataService(packet.LedgerType);
				chainDataService.Add(packet);
			}
			catch (Exception ex)
			{
				_logger.Error($"Storing packet {packet.GetType().Name} failed", ex);
			}
        }
    }
}
