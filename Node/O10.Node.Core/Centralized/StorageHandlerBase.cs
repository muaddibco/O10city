using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Node.DataLayer.DataServices;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.States;
using O10.Network.Interfaces;
using O10.Node.Core.Common;

namespace O10.Node.Core.Centralized
{
	public abstract class StorageHandlerBase<T> : IBlocksHandler where T : PacketBase
    {
        private readonly INodeContext _nodeContext;
        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly IChainDataServicesManager _chainDataServicesManager;
		private readonly IRealTimeRegistryService _realTimeRegistryService;
		private readonly ILogger _logger;
		private ActionBlock<T> _storeBlock;
        private CancellationToken _cancellationToken;

        public StorageHandlerBase(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, IChainDataServicesManager chainDataServicesManager, IRealTimeRegistryService realTimeRegistryService, ILoggerService loggerService)
        {
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _communicationServicesRegistry = communicationServicesRegistry;
            _chainDataServicesManager = chainDataServicesManager;
			_realTimeRegistryService = realTimeRegistryService;
			_logger = loggerService.GetLogger(GetType().Name);
		}

        public abstract string Name { get; }

        public abstract LedgerType PacketType { get; }

        public void Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _storeBlock = new ActionBlock<T>(StoreBlock, new ExecutionDataflowBlockOptions { BoundedCapacity = int.MaxValue,  CancellationToken = _cancellationToken, MaxDegreeOfParallelism = 1 });
        }

        public void ProcessBlock(PacketBase blockBase)
        {
            _storeBlock.Post((T)blockBase);
			_realTimeRegistryService.PostTransaction(blockBase);
        }

        private void StoreBlock(T blockBase)
        {
			_logger.Debug($"Storing packet {blockBase.GetType().Name}");

			try
			{
				IChainDataService chainDataService = _chainDataServicesManager.GetChainDataService((LedgerType)blockBase.LedgerType);
				chainDataService.Add(blockBase);
			}
			catch (Exception ex)
			{
				_logger.Error($"Storing packet {blockBase.GetType().Name} failed", ex);
			}
        }
    }
}
