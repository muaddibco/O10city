﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.States;
using O10.Core.Logging;
using O10.Network.Synchronization;
using System.Linq;
using O10.Core.HashCalculations;
using O10.Core;
using O10.Node.Core.Common;
using O10.Node.DataLayer.DataServices;
using Newtonsoft.Json;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Serialization;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using O10.Core.Identity;
using O10.Network.Interfaces;
using O10.Network.Handlers;
using O10.Network.Synchronization;

namespace O10.Node.Core.Centralized
{
    [RegisterExtension(typeof(ILedgerPacketsHandler), Lifetime = LifetimeManagement.Scoped)]
    public class TransactionsRegistryCentralizedHandler : ILedgerPacketsHandler, IDisposable
    {
        public const string NAME = "TransactionsRegistryCentralized";

        public string Name => NAME;

        public LedgerType LedgerType => LedgerType.Registry;

        private readonly SemaphoreSlim _sync = new SemaphoreSlim(1);
        private readonly IRealTimeRegistryService _realTimeRegistryService;
        private readonly INodeContext _nodeContext;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IChainDataService _synchronizationChainDataService;
        private readonly IChainDataService _registryChainDataService;
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private readonly ILogger _logger;

        private bool _isInitialized;
        private BufferBlock<IPacketBase> _packetsBuffer;
        private SynchronizationPacket _lastCombinedBlock;
        private bool _disposedValue;

        public TransactionsRegistryCentralizedHandler(IRealTimeRegistryService realTimeRegistryService,
                                                      IStatesRepository statesRepository,
                                                      IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                                      IChainDataServicesRepository chainDataServicesManager,
                                                      IHashCalculationsRepository hashCalculationsRepository,
                                                      IHandlingFlowContext handlingFlowContext,
                                                      ILoggerService loggerService)
        {
            _realTimeRegistryService = realTimeRegistryService;
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _nodeContext = statesRepository.GetInstance<INodeContext>();
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _synchronizationChainDataService = chainDataServicesManager.GetInstance(LedgerType.Synchronization);
            _registryChainDataService = chainDataServicesManager.GetInstance(LedgerType.Registry);
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _logger = loggerService.GetLogger($"{nameof(TransactionsRegistryCentralizedHandler)}#{handlingFlowContext.Index}");
            _logger.Debug(() => $"Creating {nameof(TransactionsRegistryCentralizedHandler)}...");
        }

        public async Task Initialize(CancellationToken ct)
        {
            if(_isInitialized)
            {
                return;
            }

            await _sync.WaitAsync();

            try
            {
                if (_isInitialized)
                {
                    return;
                }

                _packetsBuffer = new BufferBlock<IPacketBase>(new DataflowBlockOptions() { CancellationToken = ct });
                _lastCombinedBlock = await _synchronizationChainDataService.Single<SynchronizationPacket>(new SingleByBlockTypeKey(TransactionTypes.Synchronization_RegistryCombinationBlock), ct);
                _synchronizationContext.LastRegistrationCombinedBlockHeight = _lastCombinedBlock?.Payload.Height ?? 0;
                var synchronizationConfirmedBlock = await _synchronizationChainDataService.Single<SynchronizationPacket>(new SingleByBlockTypeKey(TransactionTypes.Synchronization_ConfirmedBlock), ct);
                if (synchronizationConfirmedBlock != null)
                {
                    _synchronizationContext.UpdateLastSyncBlockDescriptor(new SynchronizationDescriptor(
                                        synchronizationConfirmedBlock.Payload.Height,
                                        _identityKeyProvider.GetKey(_defaultTransactionHashCalculation.CalculateHash(synchronizationConfirmedBlock.ToByteArray())),
                                        synchronizationConfirmedBlock.Payload.ReportedTime,
                                        DateTime.UtcNow,
                                        synchronizationConfirmedBlock.Transaction<SynchronizationConfirmedTransaction>().Round));
                }

                _logger.LogIfDebug(() => $"{nameof(Initialize)}, {nameof(_lastCombinedBlock)}: {JsonConvert.SerializeObject(_lastCombinedBlock, new ByteArrayJsonConverter())}");

                ConsumePackets(_packetsBuffer, ct);

                _isInitialized = true;

            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task ProcessPacket(IPacketBase packet)
        {
            await _packetsBuffer.SendAsync(packet);
        }

        private async Task ConsumePackets(IReceivableSourceBlock<IPacketBase> source, CancellationToken cancellationToken)
        {
            while (await source.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
            {
				try
				{
					if (source.TryReceiveAll(out IList<IPacketBase> packets))
					{
						long lastCombinedBlockHeight = _lastCombinedBlock?.Payload.Height ?? 0;

						if (lastCombinedBlockHeight % 100 == 0)
						{
                            SynchronizationPacket synchronizationConfirmedBlock 
                                = CreateSynchronizationConfirmedBlock(
                                    _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0, 
                                    _synchronizationContext.LastBlockDescriptor?.Hash);
							_synchronizationContext.UpdateLastSyncBlockDescriptor(
                                new SynchronizationDescriptor(
                                    synchronizationConfirmedBlock.Payload.Height, 
                                    _identityKeyProvider.GetKey(_defaultTransactionHashCalculation.CalculateHash(synchronizationConfirmedBlock.ToByteArray())), 
                                    synchronizationConfirmedBlock.Payload.ReportedTime, 
                                    DateTime.UtcNow, 
                                    synchronizationConfirmedBlock.Transaction<SynchronizationConfirmedTransaction>().Round));
							_synchronizationChainDataService.Add(synchronizationConfirmedBlock);
						}

						RegistryPacket fullRegistrationsPacket 
                            = CreateRegistryFullBlock(
                                packets.Where(p => p is RegistryPacket).Cast<RegistryPacket>().ToArray(), 
                                _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0, 
                                lastCombinedBlockHeight);

                        fullRegistrationsPacket.Signature = (SingleSourceSignature)_nodeContext.SigningService.Sign(fullRegistrationsPacket.Payload);

                        var aggregatedRegistrationsPacket = CreateCombinedBlock(fullRegistrationsPacket);

						_lastCombinedBlock = aggregatedRegistrationsPacket;

						_synchronizationChainDataService.Add(aggregatedRegistrationsPacket);
						_registryChainDataService.Add(fullRegistrationsPacket);

						await _realTimeRegistryService.PostPackets(aggregatedRegistrationsPacket, fullRegistrationsPacket, cancellationToken);
					}
				}
				catch (Exception ex)
				{
					_logger.Error("Failure during processing packets for broadcasting", ex);
				}
            }
        }

        private SynchronizationPacket CreateSynchronizationConfirmedBlock(long prevSyncBlockHeight, IKey prevSyncBlockHash)
        {
            SynchronizationPacket synchronizationConfirmed = new SynchronizationPacket
            {
                Payload = new SynchronizationPayload
                {
                    Height = prevSyncBlockHeight + 1,
                    HashPrev = prevSyncBlockHash,
                    ReportedTime = DateTime.UtcNow,
                    Transaction = new SynchronizationConfirmedTransaction
                    {
                        Round = 1,
                        PublicKeys = Array.Empty<byte[]>(),// retransmittedSyncBlocks.Select(b => b.ConfirmationPublicKey).ToArray(),
                        Signatures = Array.Empty<byte[]>() //retransmittedSyncBlocks.Select(b => b.ConfirmationSignature).ToArray()
                    }
                }
            };

            synchronizationConfirmed.Signature = (SingleSourceSignature)_nodeContext.SigningService.Sign(synchronizationConfirmed.Payload);

            return synchronizationConfirmed;
        }

        private RegistryPacket CreateRegistryFullBlock(RegistryPacket[] witnesses, long syncBlockHeight, long registryFullBlockHeight)
        {
            RegistryPacket transactionsFullBlock = new RegistryPacket
            {
                Payload = new RegistryPayload
                {
                    SyncHeight = syncBlockHeight,
                    Height = registryFullBlockHeight,
                    Transaction = new FullRegistryTransaction
                    {
                        Witnesses = witnesses
                    }
                }
            };

            _logger.Debug($"Created {nameof(FullRegistryTransaction)} at syncHeight {syncBlockHeight} and height {registryFullBlockHeight} with {witnesses.Length} witnesses");

            return transactionsFullBlock;
        }

        private SynchronizationPacket CreateCombinedBlock(params RegistryPacket[] registryFullBlocks)
        {
            lock (_synchronizationContext)
            {
                var prevHash = _identityKeyProvider.GetKey(_lastCombinedBlock != null ? _defaultTransactionHashCalculation.CalculateHash(_lastCombinedBlock.ToByteArray()) : new byte[Globals.DEFAULT_HASH_SIZE]);

                //TODO: For initial POC there will be only one participant at Synchronization Layer, thus combination of FullBlocks won't be implemented fully
                SynchronizationPacket synchronizationRegistryCombinedBlock = new SynchronizationPacket
                {
                    Payload = new SynchronizationPayload
                    {
                        Height = ++_synchronizationContext.LastRegistrationCombinedBlockHeight,
                        HashPrev = prevHash,
                        ReportedTime = DateTime.Now,
                        Transaction = new AggregatedRegistrationsTransaction
                        {
                            SyncHeight = _synchronizationContext.LastBlockDescriptor?.BlockHeight ?? 0,
                            BlockHashes = registryFullBlocks.Select(b => _defaultTransactionHashCalculation.CalculateHash(b?.ToByteArray() ?? new byte[Globals.DEFAULT_HASH_SIZE])).ToArray()
                        }
                    }
                };

                synchronizationRegistryCombinedBlock.Signature = (SingleSourceSignature)_nodeContext.SigningService.Sign(synchronizationRegistryCombinedBlock.Payload);

                return synchronizationRegistryCombinedBlock;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.Debug(() => $"Stopping {nameof(TransactionsRegistryCentralizedHandler)}...");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~TransactionsRegistryCentralizedHandler()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
