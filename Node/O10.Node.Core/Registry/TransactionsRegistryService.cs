using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Serializers;
using O10.Network.Interfaces;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Predicates;
using O10.Core.States;
using O10.Core.Synchronization;
using O10.Core.HashCalculations;
using O10.Core.Logging;
using System.Threading;
using O10.Core;
using System.Threading.Tasks;
using O10.Node.Core.Common;
using O10.Core.Tracking;

namespace O10.Node.Core.Registry
{
    [RegisterDefaultImplementation(typeof(ITransactionsRegistryService), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryService : ITransactionsRegistryService
    {
        private readonly int _registrationPeriodMsec = 500;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IPredicate _isBlockProducerPredicate;
        private readonly IRegistryMemPool _registryMemPool;
        private readonly IRegistryGroupState _registryGroupState;
        private readonly IServerCommunicationServicesRegistry _serverCommunicationServicesRegistry;
        private readonly ITrackingService _trackingService;
        private readonly ISerializersFactory _serializersFactory;
        private readonly INodeContext _nodeContext;
        private readonly IHashCalculation _powCalculation;
        private readonly IHashCalculation _hashCalculation;
        private IRegistryConfiguration _registryConfiguration;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger _logger;
        private IServerCommunicationService _tcpCommunicationService;
        private IDisposable _syncContextUnsubscriber;
        private SyncCycleDescriptor _syncCycleDescriptor = null;

        public TransactionsRegistryService(IStatesRepository statesRepository, IPredicatesRepository predicatesRepository, IRegistryMemPool registryMemPool, IConfigurationService configurationService, 
            IServerCommunicationServicesRegistry serverCommunicationServicesRegistry, ITrackingService trackingService, 
            ISerializersFactory serializersFactory, IHashCalculationsRepository hashCalculationsRepository, ILoggerService loggerService)
        {
            _configurationService = configurationService;
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _registryGroupState = statesRepository.GetInstance<IRegistryGroupState>();
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _isBlockProducerPredicate = predicatesRepository.GetInstance("IsBlockProducer");
            _serverCommunicationServicesRegistry = serverCommunicationServicesRegistry;
            _trackingService = trackingService;
            _serializersFactory = serializersFactory;
            _powCalculation = hashCalculationsRepository.Create(Globals.POW_TYPE);
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _logger = loggerService.GetLogger(nameof(TransactionsRegistryService));

            _registryMemPool = registryMemPool;
        }

        public void Initialize()
        {
            _registryConfiguration = _configurationService.Get<IRegistryConfiguration>();
            _tcpCommunicationService = _serverCommunicationServicesRegistry.GetInstance(_registryConfiguration.TcpServiceName);
        }

        public void Start()
        {
            _syncContextUnsubscriber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>((Action<string>)OnSyncContextChanged));
        }

        public void Stop()
        {
            _syncCycleDescriptor?.CancellationTokenSource?.Cancel();
            _syncContextUnsubscriber?.Dispose();
        }

        private void OnSyncContextChanged(string propName)
        {
            RecalculateProductionTimer();
        }

        private void RecalculateProductionTimer()
        {
            _registryGroupState.Round = 0;

            if(_syncCycleDescriptor != null)
            {
                _syncCycleDescriptor.CancellationTokenSource.Cancel(false);
            }

            _syncCycleDescriptor = new SyncCycleDescriptor(_synchronizationContext.LastBlockDescriptor);

            PeriodicTaskFactory.Start(o => 
            {
                SyncCycleDescriptor syncCycleDescriptor = (SyncCycleDescriptor)o;
				if(syncCycleDescriptor.CancellationTokenSource.IsCancellationRequested)
				{
					return;
				}

				lock (syncCycleDescriptor)
				{
					if (syncCycleDescriptor.CancellationTokenSource.IsCancellationRequested)
					{
						return;
					}

					SortedList<ushort, RegistryRegisterBlock> transactionStateWitnesses = _registryMemPool.DequeueStateWitnessBulk();
					SortedList<ushort, RegistryRegisterStealth> transactionUtxoWitnesses = _registryMemPool.DequeueUtxoWitnessBulk();

					RegistryFullBlock registryFullBlock = ProduceTransactionsFullBlock(transactionStateWitnesses, transactionUtxoWitnesses, syncCycleDescriptor.SynchronizationDescriptor, syncCycleDescriptor.Round);
					RegistryShortBlock registryShortBlock = ProduceTransactionsShortBlock(registryFullBlock);

					if (!syncCycleDescriptor.CancellationTokenSource.IsCancellationRequested)
					{
						SendTransactionsBlocks(registryFullBlock, registryShortBlock, syncCycleDescriptor.CancellationTokenSource.Token);

						syncCycleDescriptor.Round++;
					}
				}
            }, _syncCycleDescriptor, _registrationPeriodMsec * _registryConfiguration.TotalNodes, _registryConfiguration.Position * _registrationPeriodMsec, cancelToken: _syncCycleDescriptor.CancellationTokenSource.Token, periodicTaskCreationOptions: TaskCreationOptions.LongRunning);
        }


        private RegistryFullBlock ProduceTransactionsFullBlock(SortedList<ushort, RegistryRegisterBlock> transactionStateWitnesses, SortedList<ushort, RegistryRegisterStealth> transactionUtxoWitnesses, SynchronizationDescriptor synchronizationDescriptor, int round)
        {
            ulong syncBlockHeight = synchronizationDescriptor?.BlockHeight ?? 0;
            byte[] hash = synchronizationDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE];
            byte[] pow = _powCalculation.CalculateHash(hash);
            ulong blockHeight = (ulong)(round * _registryConfiguration.TotalNodes + _registryConfiguration.Position + 1);
            RegistryRegisterBlock[] registryRegisterBlocks = transactionStateWitnesses.Select(t => t.Value).ToArray();
            RegistryRegisterStealth[] registryRegisterStealths = transactionUtxoWitnesses.Select(t => t.Value).ToArray();

            RegistryFullBlock transactionsFullBlock = new RegistryFullBlock
            {
                SyncBlockHeight = syncBlockHeight,
                PowHash = pow,
                BlockHeight = blockHeight,
                StateWitnesses = registryRegisterBlocks,
                UtxoWitnesses = registryRegisterStealths
            };

            _logger.Debug($"Created RegistryFullBlock[{syncBlockHeight}:{blockHeight}]: {registryRegisterBlocks.Length} : {registryRegisterStealths.Length}");

            return transactionsFullBlock;
        }

        private void SendTransactionsBlocks(RegistryFullBlock transactionsFullBlock, RegistryShortBlock transactionsShortBlock, CancellationToken cancellationToken)
        {
            ISerializer fullBlockSerializer = _serializersFactory.Create(transactionsFullBlock);
            ISerializer shortBlockSerializer = _serializersFactory.Create(transactionsShortBlock);

            shortBlockSerializer.SerializeBody();
            _nodeContext.SigningService.Sign(transactionsShortBlock);

            shortBlockSerializer.SerializeFully();
            transactionsFullBlock.ShortBlockHash = _hashCalculation.CalculateHash(transactionsShortBlock.RawData);

            fullBlockSerializer.SerializeBody();
            _nodeContext.SigningService.Sign(transactionsFullBlock);

            _logger.Debug($"Sending FullBlock with {transactionsFullBlock.StateWitnesses.Length + transactionsFullBlock.UtxoWitnesses.Length} transactions and ShortBlock with {transactionsShortBlock.WitnessStateKeys.Length + transactionsShortBlock.WitnessUtxoKeys.Length} keys at round {transactionsFullBlock.BlockHeight}");

			if (!cancellationToken.IsCancellationRequested)
			{
				_tcpCommunicationService.PostMessage(_registryGroupState.SyncLayerNode, fullBlockSerializer);
				_tcpCommunicationService.PostMessage(_registryGroupState.GetAllNeighbors(), shortBlockSerializer);
			}
        }

        private RegistryShortBlock ProduceTransactionsShortBlock(RegistryFullBlock transactionsFullBlock)
        {
            RegistryShortBlock transactionsShortBlock = new RegistryShortBlock
            {
                SyncBlockHeight = transactionsFullBlock.SyncBlockHeight,
                Nonce = transactionsFullBlock.Nonce,
                PowHash = transactionsFullBlock.PowHash,
                BlockHeight = transactionsFullBlock.BlockHeight,
                WitnessStateKeys = transactionsFullBlock.StateWitnesses.Select(w => new WitnessStateKey { PublicKey = w.Signer, Height = w.BlockHeight}).ToArray(),
                WitnessUtxoKeys = transactionsFullBlock.UtxoWitnesses.Select(w => new WitnessUtxoKey { KeyImage = w.KeyImage }).ToArray()
            };

            _logger.Debug($"Created RegistryShortBlock[{transactionsShortBlock.SyncBlockHeight}:{transactionsShortBlock.BlockHeight}]: {transactionsShortBlock.WitnessStateKeys.Length} : {transactionsShortBlock.WitnessUtxoKeys.Length}");

            return transactionsShortBlock;
        }
    }
}
