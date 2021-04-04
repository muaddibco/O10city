using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers.Stealth;
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
using System.Net.Http;
using System.Collections.Concurrent;
using O10.Core.Serialization;
using O10.Core.Notifications;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.DTOs;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using O10.Gateway.Common.Services.LedgerSynchronizers;

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

			_logger.LogIfDebug(() => $"Sending to Node {_synchronizerConfiguration.NodeApiUri} packet {wrapper.State.GetType().Name}");

            try
            {
				_synchronizerConfiguration.NodeApiUri
					.AppendPathSegment("Packet")
					.PostJsonAsync(wrapper.State)
					.ContinueWith(async (t, o) => 
					{
						var w = o as TaskCompletionWrapper<IPacketBase>;
						var response = await t.ConfigureAwait(false);
						if (response.IsSuccessStatusCode)
						{
							_logger.Debug($"Transaction packet posted to Node successful");
							w.TaskCompletion.SetResult(new SucceededNotification());
						}
						else
						{
							w.TaskCompletion.SetResult(new FailedNotification());
							_logger.Error($"Transaction packet posted to Node with HttpStatusCode: {response.StatusCode}, reason: \"{response.ReasonPhrase}\", content: \"{response.Content.ReadAsStringAsync().Result}\"");
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

				HttpResponseMessage response1 = await _synchronizerConfiguration.NodeApiUri.PostStringAsync(esapedTransaction.ToHexString()).ConfigureAwait(false);
				if (response1.IsSuccessStatusCode)
				{
					_logger.Debug($"Transaction packet posted to Node");
				}
				else
				{
					_logger.Error($"Transaction packet posted to Node with HttpStatusCode: {response1.StatusCode}, reason: \"{response1.ReasonPhrase}\", content: \"{response1.Content.ReadAsStringAsync().Result}\"");
				}

				HttpResponseMessage response2 = await _synchronizerConfiguration.NodeApiUri.PostStringAsync(esapedWitness.ToHexString()).ConfigureAwait(false);
				if (response2.IsSuccessStatusCode)
				{
					_logger.Debug($"Witness packet posted to Node");
				}
				else
				{
					_logger.Error($"Witness packet posted to Node with HttpStatusCode: {response2.StatusCode}, reason: \"{response2.ReasonPhrase}\", content: \"{response2.Content.ReadAsStringAsync().Result}\"");
				}

				return response1.IsSuccessStatusCode && response2.IsSuccessStatusCode;
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

        public async Task<IEnumerable<InfoMessage>> GetConnectedNodesInfo() => await _synchronizerConfiguration.NodeApiUri.GetJsonAsync<IEnumerable<InfoMessage>>(_cancellationToken).ConfigureAwait(false);

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

			if ((_lastSyncDescriptor?.Height ?? 0) < rtPackage.AggregatedRegistrations.With<AggregatedRegistrationsTransaction>().SyncHeight)
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

			var aggregatedRegistrations = rtPackage.AggregatedRegistrations.With<AggregatedRegistrationsTransaction>();
			var registryFullBlock = rtPackage.FullRegistrations.With<FullRegistryTransaction>();

			try
			{
				List<Task<WitnessPacket>> transactionStoreCompletionSources = new List<Task<WitnessPacket>>();

				ProcessRegistryFullBlock(aggregatedRegistrations, registryFullBlock, transactionStoreCompletionSources);

				SetWitnessTasks(aggregatedRegistrations, transactionStoreCompletionSources);

				StoreRegistryCombinedBlock(aggregatedRegistrations);
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure during obtaining transactions at Registry Combined Block with height {aggregatedRegistrations.Height}", ex);
			}
		}

		public async Task SendEphemeralPacket(Transactions.Core.Ledgers.Stealth.StealthPacket packet)
        {

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
					var gateways = await _synchronizerConfiguration.NodeServiceApiUri.AppendPathSegment("Gateways").GetJsonAsync<IEnumerable<GatewayDto>>().ConfigureAwait(false);
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
				if (syncBlockModel != null)
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
					_dataAccessService.CutExcessedPackets((long)registryCombinedBlock.Height);
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

        private void StoreRegistryCombinedBlock(AggregatedRegistrationsTransaction combinedBlock)
        {
            _dataAccessService.StoreRegistryCombinedBlock(combinedBlock.Height, combinedBlock.ToString());

            if ((_lastCombinedBlockDescriptor?.Height ?? 0) < combinedBlock.Height)
            {
				if(_lastCombinedBlockDescriptor == null)
                {
					_lastCombinedBlockDescriptor = new AggregatedRegistrationsTransactionDTO();
				}
                _lastCombinedBlockDescriptor.Height = combinedBlock.Height;
            }
        }

        private void SetWitnessTasks(AggregatedRegistrationsTransaction combinedBlock, List<Task<WitnessPacket>> transactionStoreCompletionSources)
        {
            if ((transactionStoreCompletionSources?.Count ?? 0) > 0)
            {
				_logger.Info($"Waiting for storing {transactionStoreCompletionSources?.Count} packets...");
                Task.WhenAll(transactionStoreCompletionSources).ContinueWith((t, o) =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
                        _logger.Info($"Packets were stored successfully");
                        WitnessPackage witnessPackage = new WitnessPackage
                        {
                            Witnesses = t.Result.Select(w => GetPacketWitness(w)),
                            CombinedBlockHeight = (long)o
                        };
                        _logger.Info($"Sending witnesses at height {witnessPackage.CombinedBlockHeight}: {string.Join(",", witnessPackage.Witnesses?.Select(w => w.WitnessId))}");
                        PipeOut?.SendAsync(witnessPackage);
                    }
                    else
					{
                        _logger.Error($"Packets were not stored successfully", t.Exception);
					}
				}, combinedBlock.Height, TaskScheduler.Current);
            }
        }

        private static PacketWitness GetPacketWitness(WitnessPacket w) => new PacketWitness
        {
            WitnessId = w.WitnessPacketId,
            DestinationKey = w.ReferencedDestinationKey?.HexStringToByteArray(),
            TransactionKey = w.ReferencedTransactionKey?.HexStringToByteArray(),
            DestinationKey2 = w.ReferencedDestinationKey2?.HexStringToByteArray(),
            KeyImage = w.ReferencedKeyImage?.HexStringToByteArray(),
            IsIdentityIssuing =
                    w.ReferencedLedgerType != LedgerType.Stealth &&
                    (
                        w.ReferencedPacketType == TransactionTypes.Transaction_IssueBlindedAsset ||
                        w.ReferencedPacketType == TransactionTypes.Transaction_IssueAssociatedBlindedAsset ||
                        w.ReferencedPacketType == TransactionTypes.Transaction_TransferAssetToStealth
                    )
        };

        private void ProcessRegistryFullBlock(AggregatedRegistrationsTransaction combinedBlock,
                                              FullRegistryTransaction registryFullBlock,
                                              List<Task<WitnessPacket>> transactionStoreCompletionSources)
        {
            _logger.Info($"[Node -> GW]: Obtained {registryFullBlock.Witnesses.Length} witnesses...");

			if (transactionStoreCompletionSources == null)
			{
				throw new ArgumentNullException(nameof(transactionStoreCompletionSources));
			}

			try
			{
                if (registryFullBlock.Witnesses != null)
                {
                    foreach (var item in registryFullBlock.Witnesses)
                    {
                        TaskCompletionSource<WitnessPacket> witnessStoreCompletionSource 
							= _dataAccessService.StoreWitnessPacket(
                                registryFullBlock.SyncHeight,
                                registryFullBlock.Height,
                                combinedBlock.Height,
                                item.With<RegisterTransaction>().ReferencedLedgerType,
                                item.With<RegisterTransaction>().ReferencedAction,
                                item.With<RegisterTransaction>().Parameters.OptionalKey("BodyHash", _identityKeyProvider),
                                item.With<RegisterTransaction>().Parameters.OptionalKey("ReferencedTarget", _identityKeyProvider),
                                item.With<RegisterTransaction>().Parameters.OptionalKey("ReferencedTarget2", _identityKeyProvider),
                                item.With<RegisterTransaction>().Parameters.OptionalKey("ReferencedTransactionKey", _identityKeyProvider),
                                item.With<RegisterTransaction>().Parameters.OptionalKey("ReferencedKeyImage", _identityKeyProvider));

                        TaskCompletionSource<WitnessPacket> transactionStoreCompletionSource 
							= ObtainAndStoreTransaction(witnessStoreCompletionSource);

                        transactionStoreCompletionSources.Add(transactionStoreCompletionSource.Task);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure at {nameof(ProcessRegistryFullBlock)}", ex);
                throw;
            }        
        }

		private TaskCompletionSource<WitnessPacket> ObtainAndStoreTransaction(TaskCompletionSource<WitnessPacket> witnessStoreCompletionSource)
		{
			TaskCompletionSource<WitnessPacket> transactionStoreCompletionSource = new TaskCompletionSource<WitnessPacket>();

			witnessStoreCompletionSource.Task.ContinueWith(async (t, o) =>
			{
				WitnessPacket witnessPacket = t.Result;
				TaskCompletionSource<WitnessPacket> completionSource = (TaskCompletionSource<WitnessPacket>)o;

                try
                {
					await _ledgerSynchronizersRepository
						.GetInstance(witnessPacket.ReferencedLedgerType)
						.SyncByWitness(witnessPacket)
						.ConfigureAwait(false);
					
					completionSource.SetResult(witnessPacket);
				}
				catch(AggregateException ex)
                {
					completionSource.SetException(ex.InnerException);
				}
				catch (Exception ex)
                {
					completionSource.SetException(ex);
				}
			}, transactionStoreCompletionSource, TaskScheduler.Current);

			return transactionStoreCompletionSource;
		}

		public IEnumerable<WitnessPackage> GetWitnessRange(long combinedBlockHeightStart, long combinedBlockHeightEnd = 0)
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
					Dictionary<long, List<WitnessPacket>> witnessPackets = _dataAccessService.GetWitnessPackets((long)combinedBlockHeightStart, (long)combinedBlockHeightEnd);
					foreach (long key in witnessPackets.Keys)
					{
						WitnessPackage witnessPackage = new WitnessPackage
						{
							CombinedBlockHeight = (long)key,
							Witnesses = witnessPackets[key].Select(w => GetPacketWitness(w)),
						};

						witnessPackages.Add(witnessPackage);
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

		public async Task<IEnumerable<IPacketBase>> GetPackets(IEnumerable<long> witnessIds)
		{
            if (witnessIds is null)
            {
                throw new ArgumentNullException(nameof(witnessIds));
            }

            _logger.Debug($"Getting packets for witnesses [{string.Join(',',witnessIds)}]");

			try
			{
				List<IPacketBase> packets = new List<IPacketBase>();

				foreach (long witnessId in witnessIds)
				{
					_logger.Info($"Getting packet for witness with id {witnessId}");
					
					WitnessPacket witness = _dataAccessService.GetWitnessPacket(witnessId);

					_logger.LogIfDebug(() => $"{nameof(WitnessPacket)}: {JsonConvert.SerializeObject(witness, new ByteArrayJsonConverter())}");

					var packet = _ledgerSynchronizersRepository.GetInstance(witness.ReferencedLedgerType).GetByWitness(witness);
					packets.Add(packet);
				}

				return await Task.FromResult(packets).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure at {nameof(GetPackets)}([{string.Join(',', witnessIds)}])", ex);
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

		#endregion
	}
}
