using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Enums;
using O10.Gateway.Common.Configuration;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using O10.Core.Serialization;
using O10.Core.Notifications;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.DTOs;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using O10.Gateway.Common.Services.LedgerSynchronizers;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Crypto.Models;

namespace O10.Gateway.Common.Services
{
    // TODO: currently NetworkSynchronizer supports only 1 Node and is not built for working against different Nodes.
    // Need to adjust the logic in the way so packets are robustly obtained from different nodes.
    [RegisterDefaultImplementation(typeof(INetworkSynchronizer), Lifetime = LifetimeManagement.Singleton)]
	public class NetworkSynchronizer : INetworkSynchronizer
	{
		private readonly ConcurrentDictionary<int, TaskCompletionSource<bool>> _connectityCheckerAwaiters;
		private readonly IDataAccessService _dataAccessService;
        private readonly ILedgerSynchronizersRepository _ledgerSynchronizersRepository;
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly IHashCalculation _defaultHashCalculation;
		private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly IAppConfig _appConfig;
        private readonly IIdentityKeyProvider _identityKeyProvider;
		private readonly ILogger _logger;
		private readonly object _sync = new object();

		private CancellationToken _cancellationToken;
		private bool _isInitialized;
		private SyncInfoDTO _lastSyncDescriptor;
		private AggregatedRegistrationsTransactionDTO _lastCombinedBlockDescriptor;

		public NetworkSynchronizer(IDataAccessService dataAccessService,
								   IHashCalculationsRepository hashCalculationsRepository,
								   ILedgerSynchronizersRepository ledgerSynchronizersRepository,
								   IConfigurationService configurationService,
								   IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
								   IAppConfig appConfig,
								   ILoggerService loggerService)
		{
			_dataAccessService = dataAccessService;
            _ledgerSynchronizersRepository = ledgerSynchronizersRepository;
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();
			_defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
			_identityKeyProvidersRegistry = identityKeyProvidersRegistry;
            _appConfig = appConfig;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
			_logger = loggerService.GetLogger(nameof(NetworkSynchronizer));

			PipeIn = new ActionBlock<TaskCompletionWrapper<IPacketBase>>(p => SendPacket(p));

			_connectityCheckerAwaiters = new ConcurrentDictionary<int, TaskCompletionSource<bool>>();
		}

		#region ============ PUBLIC FUNCTIONS =============  

		public DateTime LastSyncTime { get; set; }
		
		public IPropagatorBlock<WitnessPackage, WitnessPackage> PipeOut { get; set; }

		public ITargetBlock<TaskCompletionWrapper<IPacketBase>> PipeIn { get; }

		public async Task<AggregatedRegistrationsTransactionDTO> GetLastRegistryCombinedBlock() => await Task.FromResult(_lastCombinedBlockDescriptor).ConfigureAwait(false);

		public void SendPacket(TaskCompletionWrapper<IPacketBase> wrapper)
        {
			if(wrapper == null)
            {
				_logger.Error("Null packet received for sending");
				return;
            }

			_logger.LogIfDebug(() => $"Sending to Node {_synchronizerConfiguration.NodeApiUri} packet {wrapper.State.GetType().Name} with a transaction {wrapper.State.Transaction.GetType().FullName}");

            try
            {
				_synchronizerConfiguration.NodeApiUri
					.AppendPathSegment("Packet")
					.PostJsonAsync(wrapper.State)
					.ContinueWith((t, o) => 
					{
						var w = o as TaskCompletionWrapper<IPacketBase>;
						
						var response = t.Result;
						if (response.ResponseMessage.IsSuccessStatusCode)
						{
							_logger.Debug($"Transaction packet posted to Node successful");
							w.TryToComplete(new SucceededNotification());
						}
						else
						{
							w.TryToComplete(new FailedNotification());
							_logger.Error($"Transaction packet posted to Node with HttpStatusCode: {response.ResponseMessage.StatusCode}, reason: \"{response.ResponseMessage.ReasonPhrase}\", content: \"{response.ResponseMessage.Content.ReadAsStringAsync().Result}\"");
						}
					}, wrapper, TaskScheduler.Default);
			}
			catch (Exception ex)
            {
				_logger.Error("Failure during sending packet to node", ex);
				wrapper.TaskCompletion.SetException(ex);
            }
		}

		public async Task<bool> SendData(int transactionType, IPacketProvider packetProviderTransaction, IPacketProvider packetProviderWitness)
		{
			// 1. check transaction integrit
			try
			{
				using EscapeHelper escapeHelper = new EscapeHelper();
				byte[] esapedTransaction = escapeHelper.GetEscapedBodyBytes(packetProviderTransaction.GetBytes());
				byte[] esapedWitness = escapeHelper.GetEscapedBodyBytes(packetProviderWitness.GetBytes());

				var response1 = await _synchronizerConfiguration.NodeApiUri.PostStringAsync(esapedTransaction.ToHexString()).ConfigureAwait(false);
				if (response1.ResponseMessage.IsSuccessStatusCode)
				{
					_logger.Debug($"Transaction packet posted to Node");
				}
				else
				{
					_logger.Error($"Transaction packet posted to Node with HttpStatusCode: {response1.StatusCode}, reason: \"{response1.ResponseMessage.ReasonPhrase}\", content: \"{response1.ResponseMessage.Content.ReadAsStringAsync().Result}\"");
				}

				var response2 = await _synchronizerConfiguration.NodeApiUri.PostStringAsync(esapedWitness.ToHexString()).ConfigureAwait(false);
				if (response2.ResponseMessage.IsSuccessStatusCode)
				{
					_logger.Debug($"Witness packet posted to Node");
				}
				else
				{
					_logger.Error($"Witness packet posted to Node with HttpStatusCode: {response2.StatusCode}, reason: \"{response2.ResponseMessage.ReasonPhrase}\", content: \"{response2.ResponseMessage.Content.ReadAsStringAsync().Result}\"");
				}

				return response1.ResponseMessage.IsSuccessStatusCode && response2.ResponseMessage.IsSuccessStatusCode;
			}
			catch (Exception ex)
			{
				_logger.Error("Failed to send packet due to error", ex);
				return false;
			}
		}

		public void Initialize(CancellationToken cancellationToken)
		{
			if (_isInitialized)
			{
				return;
			}

			lock (_sync)
			{
				if (_isInitialized)
				{
					return;
				}

				ProcessInitialize(cancellationToken);
			}
		}

        public async Task<List<InfoMessage>> GetConnectedNodesInfo() => await _synchronizerConfiguration.NodeApiUri.GetJsonAsync<List<InfoMessage>>(_cancellationToken).ConfigureAwait(false);

        public void Start()
		{
		}

		public async Task<SyncInfoDTO> GetLastSyncBlock()
		{
			return await Task.FromResult(_lastSyncDescriptor).ConfigureAwait(false);
		}

		public async Task ProcessRtPackage(RtPackage rtPackage)
		{
            if (rtPackage is null)
            {
                throw new ArgumentNullException(nameof(rtPackage));
            }

            _logger.LogIfDebug(() => $"[Node -> GW]: {nameof(ProcessRtPackage)}, rtPackage: {JsonConvert.SerializeObject(rtPackage, new ByteArrayJsonConverter())}");

			if ((_lastSyncDescriptor?.Height ?? 0) < rtPackage.AggregatedRegistrations.Transaction<AggregatedRegistrationsTransaction>().SyncHeight)
			{
				try
				{
					await UpdateLastSyncBlock().ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.Error("Failure during obtaining last sync block", ex);
					return;
				}
			}

			try
			{
				List<Task<WitnessPacket>> transactionStoreCompletionSources = new List<Task<WitnessPacket>>();

				ProduceWitnessesAndStoreTransactions(rtPackage.AggregatedRegistrations, rtPackage.FullRegistrations, transactionStoreCompletionSources);

				if ((transactionStoreCompletionSources?.Count ?? 0) > 0)
				{
					_logger.Info($"Waiting for storing {transactionStoreCompletionSources?.Count} packets...");

					await Task.WhenAll(transactionStoreCompletionSources).ContinueWith((t, o) =>
					{
						if (t.IsCompletedSuccessfully)
						{
							StoreRegistryCombinedBlock((SynchronizationPacket)o);
							_logger.Info($"Processing of the aggregated registrations with the height {((SynchronizationPacket)o).Payload.Height} completed successfull");
							ProduceAndSendWitnessPackage(((SynchronizationPacket)o).Payload.Height, t.Result);
						}
						else
						{
							_logger.Error($"Processing of the aggregated registrations with the height {((SynchronizationPacket)o).Payload.Height} completed with error", t.Exception.InnerException);
						}
					}, rtPackage.AggregatedRegistrations, TaskScheduler.Current).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure during obtaining transactions at Registry Combined Block with height {rtPackage.AggregatedRegistrations.Payload.Height}", ex);
			}
		}

		public async Task<IEnumerable<WitnessPackage>> GetWitnessRange(long combinedBlockHeightStart, long combinedBlockHeightEnd = 0)
		{
			try
			{
				List<WitnessPackage> witnessPackages = new List<WitnessPackage>();

				if (combinedBlockHeightEnd == 0)
				{
					combinedBlockHeightEnd = _lastCombinedBlockDescriptor.Height - 1;
				}

				if (_lastCombinedBlockDescriptor.Height >= combinedBlockHeightStart)
                {
					bool res = await _dataAccessService.WaitUntilAggregatedRegistrationsAreStored(combinedBlockHeightStart, combinedBlockHeightEnd, TimeSpan.FromSeconds(10)).ConfigureAwait(false);

					if(!res)
                    {
						// TODO: this approach must be replaced with a mechanism making sure that GW is fully synchronized with the Node
						//throw new InvalidOperationException("Waiting for aggregated registrations storing timed out");
                    }

                    Dictionary<long, List<WitnessPacket>> witnessPackets = _dataAccessService.GetWitnessPackets(combinedBlockHeightStart, combinedBlockHeightEnd);

                    if (witnessPackets != null)
                    {
                        foreach (long key in witnessPackets.Keys)
                        {
                            WitnessPackage witnessPackage = new WitnessPackage
                            {
                                CombinedBlockHeight = key,
                                Witnesses = witnessPackets[key].Select(w => GetPacketWitness(w)).ToList(),
                            };

                            witnessPackages.Add(witnessPackage);
                        }
                    }
                }

                return witnessPackages;
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure at {nameof(GetWitnessRange)}({combinedBlockHeightStart}, {combinedBlockHeightEnd})", ex);
				throw;
			}
		}

        public async Task<IEnumerable<TransactionBase>> GetTransactions(IEnumerable<long> witnessIds)
		{
			if (witnessIds is null)
			{
				throw new ArgumentNullException(nameof(witnessIds));
			}

			_logger.Debug($"Getting transactions for witnesses [{string.Join(',', witnessIds)}]");

			try
			{
				List<TransactionBase> transactions = new List<TransactionBase>();

				foreach (long witnessId in witnessIds)
				{
					_logger.Info($"Getting transaction for witness with id {witnessId}");

					WitnessPacket witness = _dataAccessService.GetWitnessPacket(witnessId);

					if (witness != null)
					{
						_logger.LogIfDebug(() => $"{nameof(WitnessPacket)}: {JsonConvert.SerializeObject(witness, new ByteArrayJsonConverter())}");

						var packet = _ledgerSynchronizersRepository.GetInstance(witness.ReferencedLedgerType).GetByWitness(witness);
						transactions.Add(packet);
					}
					else
					{
						_logger.Error($"Failed to obtain witness, NULL obtained for witnessId {witnessId}");
					}
				}

				return await Task.FromResult(transactions).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure at {nameof(GetTransactions)}([{string.Join(',', witnessIds)}])", ex);
				throw;
			}
		}

		public async Task<StatePacketInfo> GetLastPacketInfo(IKey accountPublicKey)
		{
			if (accountPublicKey is null)
			{
				throw new ArgumentNullException(nameof(accountPublicKey));
			}

			return await GetLastPacketInfo(accountPublicKey.ToString()).ConfigureAwait(false);
		}

		public async Task<StatePacketInfo> GetLastPacketInfo(string accountPublicKey)
		{
			StatePacketInfo statePacketInfo = await _synchronizerConfiguration.NodeApiUri.AppendPathSegment("GetLastStatePacketInfo").SetQueryParam("publicKey", accountPublicKey).GetJsonAsync<StatePacketInfo>().ConfigureAwait(false);
			return statePacketInfo;
		}

		public TaskCompletionSource<bool> GetConnectivityCheckAwaiter(int nonce)
        {
			return _connectityCheckerAwaiters.GetOrAdd(nonce, new TaskCompletionSource<bool>());
		}

		public void ConnectivityCheckSet(int nonce)
        {
			if(_connectityCheckerAwaiters.TryRemove(nonce, out TaskCompletionSource<bool> awaiter))
            {
				awaiter.SetResult(true);
			}
		}

		#endregion

		#region ============ PRIVATE FUNCTIONS ============ 

		private async Task InitializeCore()
		{
			UpdateNodeRegistration();
			await UpdateLastSyncBlock().ConfigureAwait(false);
			await SynchronizeRegistryCombinedBlockToNode().ConfigureAwait(false);

			if (_dataAccessService.GetLastRegistryCombinedBlock(out long combinedBlockHeight, out string combinedBlockContent))
			{
				_lastCombinedBlockDescriptor = new AggregatedRegistrationsTransactionDTO()
				{
					Height = combinedBlockHeight
				};
			}
			else
			{
				_lastCombinedBlockDescriptor = new AggregatedRegistrationsTransactionDTO();
			}
		}

		private void ProcessInitialize(CancellationToken cancellationToken)
		{
			_logger.Info("Initialization Started");

			try
			{
				using ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(false);

				InitializeCore()
					.ContinueWith(_ => manualResetEvent.Set(), TaskScheduler.Current);

				manualResetEvent.Wait(cancellationToken);

				_cancellationToken = cancellationToken;
				_isInitialized = true;

				_logger.Info($"Last Sync Block Height = {_lastSyncDescriptor?.Height??0}; Last Registry Combined Block Height = {_lastCombinedBlockDescriptor?.Height??0}");
			}
			catch (Exception ex)
			{
				_logger.Error("Failure during initializtion", ex);
				throw;
			}
			finally
			{
				_logger.Info("Initialization completed");
			}
		}

		private async Task UpdateNodeRegistration()
		{
			_logger.Info("UpdateNodeRegistration");
			bool nodeIsDown;
			do
			{
				nodeIsDown = false;
				try
				{
					var gateways = await _synchronizerConfiguration.NodeServiceApiUri.AppendPathSegment("Gateways").GetJsonAsync<List<GatewayDto>>().ConfigureAwait(false);
					string uri = _appConfig.ReplaceToken("http://{GWSERVICENAME}/api/GatewayUpdater");
					if (gateways.All(g => g.Uri?.ToLower() != uri.ToLower()))
					{
						_logger.Info($"Registering gateway API {uri}");
						await _synchronizerConfiguration.NodeServiceApiUri.AppendPathSegment("Gateways").PostJsonAsync(new GatewayDto { Alias = _appConfig.ReplaceToken("{GWSERVICENAME}"), Uri = uri }).ConfigureAwait(false);
					}
					else
					{
						_logger.Info($"Gateway API {uri} already registered");
					}

				}
				catch (AggregateException ex)
				{
					_logger.Error("Failed to register GW at Node", ex.InnerException);
				}
				catch (Exception ex)
				{
					if (ex.InnerException?.InnerException is System.Net.Sockets.SocketException)
					{
						nodeIsDown = true;
						_logger.Warning("Node is down. Retry in 3 seconds...");
					}
					else
                    {
						_logger.Error("Failed to register GW at Node", ex);
					}
				}

				if(nodeIsDown)
                {
					await Task.Delay(3000).ConfigureAwait(false);
                }
			} while (nodeIsDown);
		}

		private async Task UpdateLastSyncBlock()
		{
			try
			{
				SyncInfoDTO syncBlockModel = await _synchronizerConfiguration.NodeApiUri.AppendPathSegment("GetLastSyncBlock").GetJsonAsync<SyncInfoDTO>().ConfigureAwait(false);
				if (syncBlockModel?.Hash != null)
				{
					_lastSyncDescriptor = syncBlockModel;
					_dataAccessService.UpdateLastSyncBlock(_lastSyncDescriptor.Height, _lastSyncDescriptor.Hash);
				}
				else
				{
					_lastSyncDescriptor = new SyncInfoDTO(0, null);
				}
			}
			catch (Exception ex)
			{
				_logger.Error("Failure during obtaining last sync block", ex);
				throw;
			}
		}

		private async Task SynchronizeRegistryCombinedBlockToNode()
		{
			try
			{
				AggregatedRegistrationsTransactionDTO registryCombinedBlock = await _synchronizerConfiguration.NodeApiUri.AppendPathSegment("LastAggregatedRegistrations").GetJsonAsync<AggregatedRegistrationsTransactionDTO>().ConfigureAwait(false);
				if (registryCombinedBlock != null)
				{
					_dataAccessService.CutExcessedPackets(registryCombinedBlock.Height);
				}
				else
				{
					_logger.Error("Failed to obtain last RegistryCombinedBlock");
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure at {nameof(SynchronizeRegistryCombinedBlockToNode)}", ex);
				throw;
			}
		}

        private void StoreRegistryCombinedBlock(SynchronizationPacket aggregatedTransactionsPacket)
        {
            _dataAccessService.StoreAggregatedRegistrations(aggregatedTransactionsPacket.Payload.Height, aggregatedTransactionsPacket.ToString());

            if ((_lastCombinedBlockDescriptor?.Height ?? 0) < aggregatedTransactionsPacket.Payload.Height)
            {
				if(_lastCombinedBlockDescriptor == null)
                {
					_lastCombinedBlockDescriptor = new AggregatedRegistrationsTransactionDTO();
				}
                _lastCombinedBlockDescriptor.Height = aggregatedTransactionsPacket.Payload.Height;
            }
        }

		private void ProduceAndSendWitnessPackage(long aggregatedTransactionsPacketHeight, WitnessPacket[] witnessPackets)
		{
			WitnessPackage witnessPackage = new WitnessPackage
			{
				Witnesses = witnessPackets.Select(w => GetPacketWitness(w)).ToList(),
				CombinedBlockHeight = aggregatedTransactionsPacketHeight
			};
			_logger.Info($"Sending witnesses at height {witnessPackage.CombinedBlockHeight}: {string.Join(",", witnessPackage.Witnesses?.Select(w => w.WitnessId))}");

			PipeOut?.SendAsync(witnessPackage);
		}

        private PacketWitness GetPacketWitness(WitnessPacket w) => new PacketWitness
        {
            WitnessId = w.WitnessPacketId,
            DestinationKey = _identityKeyProvider.GetKey(w.ReferencedDestinationKey?.HexStringToByteArray()),
            TransactionKey = _identityKeyProvider.GetKey(w.ReferencedTransactionKey?.HexStringToByteArray()),
            DestinationKey2 = _identityKeyProvider.GetKey(w.ReferencedDestinationKey2?.HexStringToByteArray()),
            KeyImage = _identityKeyProvider.GetKey(w.ReferencedKeyImage?.HexStringToByteArray()),
            IsIdentityIssuing =
                    w.ReferencedLedgerType != LedgerType.Stealth &&
                    (
                        w.ReferencedPacketType == TransactionTypes.Transaction_IssueBlindedAsset ||
                        w.ReferencedPacketType == TransactionTypes.Transaction_IssueAssociatedBlindedAsset ||
                        w.ReferencedPacketType == TransactionTypes.Transaction_TransferAssetToStealth
                    )
        };

        private void ProduceWitnessesAndStoreTransactions(SynchronizationPacket aggregatedTransactionsPacket,
                                              RegistryPacket fullRegistryTransactionsPacket,
                                              List<Task<WitnessPacket>> transactionStoreCompletionSources)
        {
			var fullRegistryTransaction = fullRegistryTransactionsPacket.Transaction<FullRegistryTransaction>();
            _logger.Info($"[Node -> GW]: Obtained {fullRegistryTransaction.Witnesses.Length} witnesses...");

			if (transactionStoreCompletionSources == null)
			{
				throw new ArgumentNullException(nameof(transactionStoreCompletionSources));
			}

			try
			{
                if (fullRegistryTransaction.Witnesses != null)
                {
                    foreach (var witness in fullRegistryTransaction.Witnesses)
                    {
                        TaskCompletionSource<WitnessPacket> witnessStoreCompletionSource 
							= _dataAccessService.StoreWitnessPacket(
                                fullRegistryTransactionsPacket.Payload.SyncHeight,
                                fullRegistryTransactionsPacket.Payload.Height,
                                aggregatedTransactionsPacket.Payload.Height,
                                witness.Transaction<RegisterTransaction>().ReferencedLedgerType,
                                witness.Transaction<RegisterTransaction>().ReferencedAction,
                                witness.Transaction<RegisterTransaction>().Parameters.OptionalKey(EvidenceDescriptor.TRANSACTION_HASH, _identityKeyProvider),
                                witness.Transaction<RegisterTransaction>().Parameters.OptionalKey(EvidenceDescriptor.REFERENCED_TARGET, _identityKeyProvider),
                                witness.Transaction<RegisterTransaction>().Parameters.OptionalKey(EvidenceDescriptor.REFERENCED_TARGET2, _identityKeyProvider),
                                witness.Transaction<RegisterTransaction>().Parameters.OptionalKey(EvidenceDescriptor.REFERENCED_TRANSACTION_KEY, _identityKeyProvider),
                                witness.Transaction<RegisterTransaction>().Parameters.OptionalKey(EvidenceDescriptor.REFERENCED_KEY_IMAGE, _identityKeyProvider));

                        var transactionStoreCompletionSource = ObtainAndStoreWitnessedTransaction(witness.Transaction<RegisterTransaction>(), witnessStoreCompletionSource);

                        transactionStoreCompletionSources.Add(transactionStoreCompletionSource.Task);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure at {nameof(ProduceWitnessesAndStoreTransactions)}", ex);
                throw;
            }        
        }

		private TaskCompletionSource<WitnessPacket> ObtainAndStoreWitnessedTransaction(RegisterTransaction registerTransaction, TaskCompletionSource<WitnessPacket> onceWithnessIsStoredToDB)
		{
			TaskCompletionSource<WitnessPacket> transactionStoreCompletionSource = new TaskCompletionSource<WitnessPacket>();

			onceWithnessIsStoredToDB.Task.ContinueWith(async (t, o) =>
			{
				WitnessPacket witnessPacket = t.Result;
				(var r, var c) = ((RegisterTransaction, TaskCompletionSource<WitnessPacket>))o;

                try
                {
					await _ledgerSynchronizersRepository
						.GetInstance(witnessPacket.ReferencedLedgerType)
						.SyncByWitness(witnessPacket, r)
						.ConfigureAwait(false);
					
					c.SetResult(witnessPacket);
				}
				catch(AggregateException ex)
                {
					c.SetException(ex.InnerException);
				}
				catch (Exception ex)
                {
					c.SetException(ex);
				}
			}, (registerTransaction, transactionStoreCompletionSource), TaskScheduler.Current);

			return transactionStoreCompletionSource;
		}

		#endregion
	}
}
