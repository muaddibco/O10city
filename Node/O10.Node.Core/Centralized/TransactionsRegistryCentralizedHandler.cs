using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Interfaces;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Core.States;
using O10.Core.Logging;
using O10.Core.Synchronization;
using System.Linq;
using O10.Core.HashCalculations;
using O10.Transactions.Core.Serializers;
using O10.Core;
using O10.Node.Core.Common;
using O10.Node.DataLayer.DataServices;
using Newtonsoft.Json;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Serialization;

namespace O10.Node.Core.Centralized
{
    [RegisterExtension(typeof(IBlocksHandler), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionsRegistryCentralizedHandler : IBlocksHandler
    {
        public const string NAME = "TransactionsRegistryCentralized";

        public string Name => NAME;

        public LedgerType PacketType => LedgerType.Registry;

        private readonly object _sync = new object();
        private readonly IRealTimeRegistryService _realTimeRegistryService;
        private readonly ISerializersFactory _serializersFactory;
        private readonly INodeContext _nodeContext;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IChainDataService _synchronizationChainDataService;
        private readonly IChainDataService _registryChainDataService;
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private readonly ILogger _logger;

        private bool _isInitialized = false;
        private BufferBlock<PacketBase> _packetsBuffer;
        private SynchronizationRegistryCombinedBlock _lastCombinedBlock;

        public TransactionsRegistryCentralizedHandler(IRealTimeRegistryService realTimeRegistryService, IStatesRepository statesRepository, IChainDataServicesManager chainDataServicesManager, ISerializersFactory serializersFactory, IHashCalculationsRepository hashCalculationsRepository, ILoggerService loggerService)
        {
            _realTimeRegistryService = realTimeRegistryService;
            _serializersFactory = serializersFactory;
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _synchronizationChainDataService = chainDataServicesManager.GetChainDataService(LedgerType.Synchronization);
            _registryChainDataService = chainDataServicesManager.GetChainDataService(LedgerType.Registry);
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _logger = loggerService.GetLogger(nameof(TransactionsRegistryCentralizedHandler));
        }

        public void Initialize(CancellationToken ct)
        {
            if(_isInitialized)
            {
                return;
            }

            lock(_sync)
            {
                if(_isInitialized)
                {
                    return;
                }

                _packetsBuffer = new BufferBlock<PacketBase>(new DataflowBlockOptions() { CancellationToken = ct });
                _lastCombinedBlock = _synchronizationChainDataService.Single<SynchronizationRegistryCombinedBlock>(new SingleByBlockTypeKey(PacketTypes.Synchronization_RegistryCombinationBlock));

                _logger.LogIfDebug(() => $"{nameof(Initialize)}, {nameof(_lastCombinedBlock)}: {JsonConvert.SerializeObject(_lastCombinedBlock, new ByteArrayJsonConverter())}");

                ConsumePackets(_packetsBuffer, ct);

                _isInitialized = true;
            }
        }

        public void ProcessBlock(PacketBase packet)
        {
            _packetsBuffer.SendAsync(packet);
        }

        private async Task ConsumePackets(IReceivableSourceBlock<PacketBase> source, CancellationToken cancellationToken)
        {
            while (await source.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
            {
				try
				{
					if (source.TryReceiveAll(out IList<PacketBase> packets))
					{
						ulong lastCombinedBlockHeight = _lastCombinedBlock?.Height ?? 0;

						if (lastCombinedBlockHeight % 100 == 0)
						{
							SynchronizationConfirmedBlock synchronizationConfirmedBlock = CreateSynchronizationConfirmedBlock(_synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0, _synchronizationContext.LastBlockDescriptor?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE]);
							_synchronizationContext.UpdateLastSyncBlockDescriptor(new SynchronizationDescriptor(synchronizationConfirmedBlock.Height, _defaultTransactionHashCalculation.CalculateHash(synchronizationConfirmedBlock.RawData), synchronizationConfirmedBlock.ReportedTime, DateTime.UtcNow, synchronizationConfirmedBlock.Round));
							_synchronizationChainDataService.Add(synchronizationConfirmedBlock);
						}

						RegistryFullBlock registryFullBlock = CreateRegistryFullBlock(
                            packets.Where(p => p is RegistryRegisterBlock).Cast<RegistryRegisterBlock>().ToArray(), 
                            packets.Where(p => p is RegistryRegisterStealth).Cast<RegistryRegisterStealth>().ToArray(), _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0, lastCombinedBlockHeight);
						RegistryShortBlock registryShortBlock = CreateRegistryShortBlock(registryFullBlock);
						SerializeRegistryBlocks(registryFullBlock, registryShortBlock);

						SynchronizationRegistryCombinedBlock combinedBlock = CreateCombinedBlock(registryFullBlock);

						_lastCombinedBlock = combinedBlock;

						_synchronizationChainDataService.Add(combinedBlock);
						_registryChainDataService.Add(registryFullBlock);

						_realTimeRegistryService.PostPackets(combinedBlock, registryFullBlock);
					}
				}
				catch (Exception ex)
				{
					_logger.Error("Failure during processing packets for broadcasting", ex);
				}
            }
        }

        private SynchronizationConfirmedBlock CreateSynchronizationConfirmedBlock(ulong prevSyncBlockHeight, byte[] prevSyncBlockHash)
        {
            SynchronizationConfirmedBlock synchronizationConfirmed = new SynchronizationConfirmedBlock
            {
                SyncHeight = prevSyncBlockHeight,
                Height = prevSyncBlockHeight + 1,
                HashPrev = prevSyncBlockHash,
                PowHash = new byte[Globals.POW_HASH_SIZE],
                ReportedTime = DateTime.UtcNow,
                Round = 1,
                PublicKeys = Array.Empty<byte[]>(),// retransmittedSyncBlocks.Select(b => b.ConfirmationPublicKey).ToArray(),
                Signatures = Array.Empty<byte[]>() //retransmittedSyncBlocks.Select(b => b.ConfirmationSignature).ToArray()
            };

            ISerializer confirmationBlockSerializer = _serializersFactory.Create(synchronizationConfirmed);
            confirmationBlockSerializer.SerializeBody();
            _nodeContext.SigningService.Sign(synchronizationConfirmed);
            confirmationBlockSerializer.SerializeFully();

            return synchronizationConfirmed;
        }

        private RegistryFullBlock CreateRegistryFullBlock(RegistryRegisterBlock[] stateWitnesses,
                                                          RegistryRegisterStealth[] stealthWitnesses,
                                                          RegistryRegisterExBlock[] witnesses,
                                                          ulong syncBlockHeight,
                                                          ulong registryFullBlockHeight)
        {
            RegistryFullBlock transactionsFullBlock = new RegistryFullBlock
            {
                SyncHeight = syncBlockHeight,
                PowHash = new byte[Globals.POW_HASH_SIZE],
                Height = registryFullBlockHeight,
                StateWitnesses = stateWitnesses,
                StealthWitnesses = stealthWitnesses,
                UniversalWitnesses = witnesses
            };

            _logger.Debug($"Created RegistryFullBlock[{syncBlockHeight}:{registryFullBlockHeight}]: {stateWitnesses.Length} : {stealthWitnesses.Length}");

            return transactionsFullBlock;
        }

        private RegistryShortBlock CreateRegistryShortBlock(RegistryFullBlock transactionsFullBlock)
        {
            RegistryShortBlock registryShortBlock = new RegistryShortBlock
            {
                SyncHeight = transactionsFullBlock.SyncHeight,
                Nonce = transactionsFullBlock.Nonce,
                PowHash = transactionsFullBlock.PowHash,
                Height = transactionsFullBlock.Height,
                WitnessStateKeys = transactionsFullBlock.StateWitnesses.Select(w => new WitnessStateKey { PublicKey = w.Signer, Height = w.Height }).ToArray(),
                WitnessUtxoKeys = transactionsFullBlock.StealthWitnesses.Select(w => new WitnessUtxoKey { KeyImage = w.KeyImage }).ToArray()
            };

            _logger.Debug($"Created RegistryShortBlock[{registryShortBlock.SyncHeight}:{registryShortBlock.Height}]: {registryShortBlock.WitnessStateKeys.Length} : {registryShortBlock.WitnessUtxoKeys.Length}");

            return registryShortBlock;
        }

        private void SerializeRegistryBlocks(RegistryFullBlock transactionsFullBlock, RegistryShortBlock transactionsShortBlock)
        {
            ISerializer fullBlockSerializer = _serializersFactory.Create(transactionsFullBlock);
            ISerializer shortBlockSerializer = _serializersFactory.Create(transactionsShortBlock);

            shortBlockSerializer.SerializeBody();
            _nodeContext.SigningService.Sign(transactionsShortBlock);
            shortBlockSerializer.SerializeFully();

            transactionsFullBlock.ShortBlockHash = _defaultTransactionHashCalculation.CalculateHash(transactionsShortBlock.RawData);
            fullBlockSerializer.SerializeBody();
            _nodeContext.SigningService.Sign(transactionsFullBlock);
            fullBlockSerializer.SerializeFully();

            _logger.Debug($"Sending FullBlock with {transactionsFullBlock.StateWitnesses.Length + transactionsFullBlock.StealthWitnesses.Length} transactions and ShortBlock with {transactionsShortBlock.WitnessStateKeys.Length + transactionsShortBlock.WitnessUtxoKeys.Length} keys at round {transactionsFullBlock.Height}");
        }

        private SynchronizationRegistryCombinedBlock CreateCombinedBlock(params RegistryFullBlock[] registryFullBlocks)
        {
            lock (_synchronizationContext)
            {
                byte[] prevHash = _lastCombinedBlock != null ? _defaultTransactionHashCalculation.CalculateHash(_lastCombinedBlock.RawData) : new byte[Globals.DEFAULT_HASH_SIZE];

                //TODO: For initial POC there will be only one participant at Synchronization Layer, thus combination of FullBlocks won't be implemented fully
                SynchronizationRegistryCombinedBlock synchronizationRegistryCombinedBlock = new SynchronizationRegistryCombinedBlock
                {
                    SyncHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0,
                    PowHash = new byte[Globals.POW_HASH_SIZE],
                    Height = ++_synchronizationContext.LastRegistrationCombinedBlockHeight,
                    HashPrev = prevHash,
                    ReportedTime = DateTime.Now,
                    BlockHashes = registryFullBlocks.Select(b => _defaultTransactionHashCalculation.CalculateHash(b?.RawData ?? new byte[Globals.DEFAULT_HASH_SIZE])).ToArray()
                };

                ISerializer combinedBlockSerializer = _serializersFactory.Create(synchronizationRegistryCombinedBlock);
                combinedBlockSerializer.SerializeBody();
                _nodeContext.SigningService.Sign(synchronizationRegistryCombinedBlock);
                combinedBlockSerializer.SerializeFully();

                return synchronizationRegistryCombinedBlock;
            }
        }
    }
}
