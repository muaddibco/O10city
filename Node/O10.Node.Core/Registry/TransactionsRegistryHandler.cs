//using System;
//using System.Collections.Concurrent;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Threading.Tasks.Dataflow;
//using O10.Transactions.Core.Ledgers.Registry;
//using O10.Transactions.Core.Enums;
//using O10.Transactions.Core.Interfaces;
//using O10.Transactions.Core.Serializers.RawPackets;
//using O10.Network.Interfaces;
//using O10.Core.Architecture;
//using O10.Core.Communication;
//using O10.Core.Configuration;
//using O10.Core.HashCalculations;
//using O10.Core.States;
//using O10.Node.Core.Common;
//using O10.Core.Synchronization;
//using O10.Core.Logging;
//using O10.Core;
//using O10.Transactions.Core.Ledgers;

//namespace O10.Node.Core.Registry
//{
//    [RegisterExtension(typeof(IPacketsHandler), Lifetime = LifetimeManagement.Singleton)]
//    public class TransactionsRegistryHandler : IPacketsHandler
//    {
//        public const string NAME = "TransactionsRegistry";

//        private readonly ITargetBlock<RegistryShortBlock> _processWitnessedFlow;
//        private readonly BlockingCollection<RegistryRegisterExBlock> _registryStateExBlocks;
//        private readonly BlockingCollection<RegistryRegisterBlock> _registryStateBlocks;
//        private readonly BlockingCollection<RegistryRegisterStealth> _registryUtxoBlocks;
//        private readonly IServerCommunicationServicesRegistry _communicationServicesRegistry;
//        private readonly IRawPacketProvidersFactory _rawPacketProvidersFactory;
//        private readonly IRegistryMemPool _registryMemPool;
//        private readonly IConfigurationService _configurationService;
//        private readonly IRegistryGroupState _registryGroupState;
//        private readonly ISynchronizationContext _synchronizationContext;
//        private readonly IHashCalculation _defaulHashCalculation;
//        private readonly IHashCalculation _powCalculation;
//        private readonly INodeContext _nodeContext;
//        private readonly ILogger _logger;
//        private IServerCommunicationService _udpCommunicationService;
//        private IServerCommunicationService _tcpCommunicationService;

//        public TransactionsRegistryHandler(IStatesRepository statesRepository, IServerCommunicationServicesRegistry communicationServicesRegistry, 
//            IRawPacketProvidersFactory rawPacketProvidersFactory, IRegistryMemPool registryMemPool, IConfigurationService configurationService, 
//            IHashCalculationsRepository hashCalculationRepository, ILoggerService loggerService)
//        {
//            _registryStateBlocks = new BlockingCollection<RegistryRegisterBlock>();
//            _registryStateExBlocks = new BlockingCollection<RegistryRegisterExBlock>();
//            _registryUtxoBlocks = new BlockingCollection<RegistryRegisterStealth>();
//            _registryGroupState = statesRepository.GetInstance<IRegistryGroupState>();
//            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
//            _nodeContext = statesRepository.GetInstance<INodeContext>();
//            _communicationServicesRegistry = communicationServicesRegistry;
//            _rawPacketProvidersFactory = rawPacketProvidersFactory;
//            _registryMemPool = registryMemPool;
//            _configurationService = configurationService;
//            _defaulHashCalculation = hashCalculationRepository.Create(Globals.DEFAULT_HASH);
//            _powCalculation = hashCalculationRepository.Create(Globals.POW_TYPE);
//            _logger = loggerService.GetLogger(nameof(TransactionsRegistryHandler));

//            _processWitnessedFlow = new ActionBlock<RegistryShortBlock>((Action<RegistryShortBlock>)ProcessWitnessed);
//        }

//        public string Name => NAME;

//        public LedgerType LedgerType => LedgerType.Registry;

//        public void Initialize(CancellationToken ct)
//        {
//            _tcpCommunicationService = _communicationServicesRegistry.GetInstance(_configurationService.Get<IRegistryConfiguration>().TcpServiceName);

//            Task.Factory.StartNew(() => {
//                ProcessStateBlocks(ct);
//            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

//            Task.Factory.StartNew(() => {
//                ProcessStateExBlocks(ct);
//            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);

//            Task.Factory.StartNew(() => {
//                ProcessUtxoBlocks(ct);
//            }, ct, TaskCreationOptions.LongRunning, TaskScheduler.Default);
//        }

//        public void ProcessBlock(PacketBase blockBase)
//        {
//            if (blockBase is RegistryRegisterBlock transactionRegisterStateBlock)
//            {
//                _registryStateBlocks.Add(transactionRegisterStateBlock);
//            }

//            if (blockBase is RegistryRegisterExBlock registerExBlock)
//            {
//                _registryStateExBlocks.Add(registerExBlock);
//            }

//            if (blockBase is RegistryRegisterStealth transactionRegisterUtxoBlock)
//            {
//                _registryUtxoBlocks.Add(transactionRegisterUtxoBlock);
//            }

//            if (blockBase is RegistryShortBlock transactionsShortBlock)
//            {
//                _processWitnessedFlow.Post(transactionsShortBlock);
//            }
//        }

//        #region Private Functions

//        private void ProcessStateBlocks(CancellationToken ct)
//        {
//            foreach (RegistryRegisterBlock transactionRegisterBlock in _registryStateBlocks.GetConsumingEnumerable(ct))
//            {
//                //TODO: add logic that will check whether received Transaction Header was already stored into blockchain

//                bool isNew = _registryMemPool.EnqueueTransactionWitness(transactionRegisterBlock);

//                if (isNew)
//                {
//                    IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(transactionRegisterBlock);
//                    _tcpCommunicationService.PostMessage(_registryGroupState.GetAllNeighbors(), packetProvider);
//                }
//            }
//        }

//        private void ProcessStateExBlocks(CancellationToken ct)
//        {
//            foreach (RegistryRegisterExBlock packet in _registryStateExBlocks.GetConsumingEnumerable(ct))
//            {
//                //TODO: add logic that will check whether received Transaction Header was already stored into blockchain

//                bool isNew = _registryMemPool.EnqueueTransactionWitness(packet);

//                if (isNew)
//                {
//                    IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(packet);
//                    _tcpCommunicationService.PostMessage(_registryGroupState.GetAllNeighbors(), packetProvider);
//                }
//            }
//        }

//        private void ProcessUtxoBlocks(CancellationToken ct)
//        {
//            foreach (RegistryRegisterStealth transactionRegisterUtxo in _registryUtxoBlocks.GetConsumingEnumerable(ct))
//            {
//                //TODO: add logic that will check whether received Transaction Header was already stored into blockchain

//                bool isNew = _registryMemPool.EnqueueTransactionWitness(transactionRegisterUtxo);

//                if (isNew)
//                {
//                    IPacketProvider packetProvider = _rawPacketProvidersFactory.Create(transactionRegisterUtxo);
//                    _tcpCommunicationService.PostMessage(_registryGroupState.GetAllNeighbors(), packetProvider);
//                }
//            }
//        }

//        private void ProcessWitnessed(RegistryShortBlock registryShortBlock)
//        {
//            _registryGroupState.ToggleLastBlockConfirmationReceived();

//            if (registryShortBlock != null)
//            {
//                _registryMemPool.ClearWitnessed(registryShortBlock);
//            }

//            //TODO: obtain Transactions Registry Short block from MemPool by hash given in confirmationBlock
//            //TODO: clear MemPool from Transaction Headers of confirmed Short Block
//        }

//        #endregion PrivateFunctions
//    }
//}
