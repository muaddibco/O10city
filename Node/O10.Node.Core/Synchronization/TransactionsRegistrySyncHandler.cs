using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Transactions.Core.Serializers;
using O10.Network.Interfaces;
using O10.Network.Topology;
using O10.Core.Architecture;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.States;
using O10.Core.Synchronization;
using O10.Transactions.Core.DataModel.Synchronization;
using O10.Transactions.Core.Serializers.RawPackets;
using O10.Core.Logging;
using O10.Core.ExtensionMethods;
using O10.Core.Configuration;
using O10.Core;
using O10.Core.Models;
using O10.Node.Core.Common;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;

namespace O10.Node.Core.Synchronization
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistrySyncHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistrySync";

        private readonly int _cyclePeriodMsec = 500;

        private readonly BlockingCollection<RegistryBlockBase> _registryBlocks;
        private readonly INodeContext _nodeContext;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ISerializersFactory _serializersFactory;
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private readonly IHashCalculation _powCalculation;
        private readonly ISyncRegistryNeighborhoodState _syncRegistryNeighborhoodState;

        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
        private readonly ISyncRegistryMemPool _syncRegistryMemPool;
        private readonly INodesResolutionService _nodesResolutionService;
        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
        private readonly IChainDataService _synchronizationChainDataService;
        private readonly IChainDataService _registryChainDataService;
        private readonly IConfigurationService _configurationService;

        private readonly ILogger _logger;

        private ISynchronizationConfiguration _synchronizationConfiguration;
        private IDisposable _syncContextChangedUnsibsciber;
        private IServerCommunicationService _communicationService;
        private CancellationToken _cancellationToken;
        private CancellationTokenSource _cancellationTokenSource = null;

		private SynchronizationRegistryCombinedBlock _lastCombinedBlock;

        public TransactionsRegistrySyncHandler(IStatesRepository statesRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ISerializersFactory serializersFactory, IHashCalculationsRepository hashCalculationsRepository,
            IServerCommunicationServicesRegistry communicationServicesRegistry, ISyncRegistryMemPool syncRegistryMemPool, INodesResolutionService nodesResolutionService,
            IChainDataServicesManager chainDataServicesManager, IRawPacketProvidersFactory rawPacketProvidersFactory,IConfigurationService configurationService, ILoggerService loggerService)
        {
            _configurationService = configurationService;
            _registryBlocks = new BlockingCollection<RegistryBlockBase>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _syncRegistryNeighborhoodState = statesRepository.GetInstance<ISyncRegistryNeighborhoodState>();
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _transactionHashKey = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
            _serializersFactory = serializersFactory;
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _powCalculation = hashCalculationsRepository.Create(Globals.POW_TYPE);
            _communicationServicesRegistry = communicationServicesRegistry;
            _syncRegistryMemPool = syncRegistryMemPool;
            _nodesResolutionService = nodesResolutionService;
            _rawPacketProvidersFactory = rawPacketProvidersFactory;
            _synchronizationChainDataService = chainDataServicesManager.GetChainDataService(PacketType.Synchronization);
            _registryChainDataService = chainDataServicesManager.GetChainDataService(PacketType.Registry);
            _logger = loggerService.GetLogger(nameof(TransactionsRegistrySyncHandler));
        }

        public string Name => NAME;

        public PacketType PacketType => PacketType.Registry;

        public void Initialize(CancellationToken ct)
        {
            _cancellationToken = ct;
            _cancellationToken.Register(() => _syncContextChangedUnsibsciber.Dispose());
            _synchronizationConfiguration = _configurationService.Get<ISynchronizationConfiguration>();
            _communicationService = _communicationServicesRegistry.GetInstance(_synchronizationConfiguration.CommunicationServiceName);
            _syncContextChangedUnsibsciber = _synchronizationContext.SubscribeOnStateChange(new ActionBlock<string>(SynchronizationStateChanged));
            _lastCombinedBlock = _synchronizationChainDataService.Single<SynchronizationRegistryCombinedBlock>(new SingleByBlockTypeKey(ActionTypes.Synchronization_RegistryCombinationBlock));

			Task.Factory.StartNew(() => ProcessBlocks(ct), ct, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        public void ProcessBlock(PacketBase blockBase)
        {
            if (blockBase == null)
            {
                throw new ArgumentNullException(nameof(blockBase));
            }

            _logger.Debug($"{nameof(TransactionsRegistrySyncHandler)} - processing block {blockBase.RawData.ToHexString()}");

            if (blockBase is RegistryFullBlock registryBlock)
            {
                _registryBlocks.Add(registryBlock);
            }
        }

        #region Private Functions

        private void SynchronizationStateChanged(string propName)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            PeriodicTaskFactory.Start(() => 
            {
                IEnumerable<RegistryFullBlock> registryFullBlocks = _syncRegistryMemPool.GetRegistryBlocks();

                CreateAndDistributeCombinedBlock(registryFullBlocks);
                DistributeAndSaveFullBlock(registryFullBlocks);
            }, _cyclePeriodMsec * _synchronizationConfiguration.TotalNodes, _cyclePeriodMsec * _synchronizationConfiguration.Position, cancelToken: _cancellationTokenSource.Token, periodicTaskCreationOptions: TaskCreationOptions.LongRunning);
        }

        private void ProcessBlocks(CancellationToken ct)
        {
            foreach (RegistryBlockBase registryBlock in _registryBlocks.GetConsumingEnumerable(ct))
            {
                try
                {
                    _logger.Debug($"Obtained block {registryBlock.GetType().Name} with Round {registryBlock.BlockHeight}");

                    lock (_syncRegistryMemPool)
                    {
                        if (registryBlock is RegistryFullBlock registryFullBlock)
                        {
                            _syncRegistryMemPool.AddCandidateBlock(registryFullBlock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Error during processing block", ex);
                }
            }
        }

        private void CreateAndDistributeCombinedBlock(IEnumerable<RegistryFullBlock> registryFullBlocks)
        {
            lock (_synchronizationContext)
            {
                byte[] prevHash = _lastCombinedBlock != null ? _defaultTransactionHashCalculation.CalculateHash(_lastCombinedBlock.RawData) : new byte[Globals.DEFAULT_HASH_SIZE];

                //TODO: For initial POC there will be only one participant at Synchronization Layer, thus combination of FullBlocks won't be implemented fully
                SynchronizationRegistryCombinedBlock synchronizationRegistryCombinedBlock = new SynchronizationRegistryCombinedBlock
                {
                    SyncBlockHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0,
                    PowHash = _powCalculation.CalculateHash(_synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE]),
                    BlockHeight = ++_synchronizationContext.LastRegistrationCombinedBlockHeight,
                    HashPrev = prevHash,
                    ReportedTime = DateTime.Now,
                    BlockHashes = registryFullBlocks.Select(b => _defaultTransactionHashCalculation.CalculateHash(b?.RawData ?? new byte[Globals.DEFAULT_HASH_SIZE])).ToArray()
                };

                ISerializer combinedBlockSerializer = _serializersFactory.Create(synchronizationRegistryCombinedBlock);
                combinedBlockSerializer.SerializeBody();
                _nodeContext.SigningService.Sign(synchronizationRegistryCombinedBlock);
                combinedBlockSerializer.SerializeFully();

                IEnumerable<IKey> storageLayerKeys = _nodesResolutionService.GetStorageNodeKeys(combinedBlockSerializer);
                _communicationService.PostMessage(storageLayerKeys, combinedBlockSerializer);

				this._lastCombinedBlock = synchronizationRegistryCombinedBlock;

				_synchronizationChainDataService.Add(synchronizationRegistryCombinedBlock);

                foreach (var item in registryFullBlocks)
                {
                    _logger.Debug($"Recombined RegistryFullBlock[{item.SyncBlockHeight}:{item.BlockHeight}]: {item.StateWitnesses.Length} : {item.UtxoWitnesses.Length}");
                }
            }
        }

        private void DistributeAndSaveFullBlock(IEnumerable<RegistryFullBlock> registryFullBlocks)
        {
            if (registryFullBlocks != null)
            {
                foreach (var registryFullBlock in registryFullBlocks)
                {
                    IRawPacketProvider fullBlockSerializer = _rawPacketProvidersFactory.Create(registryFullBlock);

                    IEnumerable<IKey> storageLayerKeys = _nodesResolutionService.GetStorageNodeKeys(fullBlockSerializer);
                    _communicationService.PostMessage(storageLayerKeys, fullBlockSerializer);

                    _registryChainDataService.Add(registryFullBlock);

                    _logger.Debug($"Stored RegistryFullBlock[{registryFullBlock.SyncBlockHeight}:{registryFullBlock.BlockHeight}]: {registryFullBlock.StateWitnesses.Length} : {registryFullBlock.UtxoWitnesses.Length}");
                }
            }
        }

        #endregion Private Functions
    }
}
