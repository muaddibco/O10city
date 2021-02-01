using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.DataModel.Synchronization;
using O10.Transactions.Core.DataModel.Transactional;
using O10.Transactions.Core.DataModel.Stealth;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Parsers;
using O10.Gateway.Common.Configuration;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services;
using O10.Gateway.DataLayer.Services.Inputs;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Transactions.Core.DataModel;
using Microsoft.AspNetCore.SignalR.Client;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using System.Net.Http;
using O10.Gateway.Common.Dtos;
using System.Collections.Concurrent;
using O10.Core.Serialization;
using O10.Core.Notifications;

namespace O10.Gateway.Common.Services
{
    [RegisterDefaultImplementation(typeof(INetworkSynchronizer), Lifetime = LifetimeManagement.Singleton)]
	public class NetworkSynchronizer : INetworkSynchronizer
	{
		private readonly ConcurrentDictionary<int, TaskCompletionSource<bool>> _connectityCheckerAwaiters;
		private readonly IDataAccessService _dataAccessService;
        private readonly ISynchronizerConfiguration _synchronizerConfiguration;
        private readonly IHashCalculation _defaultHashCalculation;
		private readonly IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
		private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly IAppConfig _appConfig;
        private readonly IIdentityKeyProvider _identityKeyProvider;
		private readonly ILogger _logger;
		private readonly object _sync = new object();

		private CancellationToken _cancellationToken;
		private bool _isInitialized;
		private SyncBlockModel _lastSyncDescriptor;
		private RegistryCombinedBlockModel _lastCombinedBlockDescriptor;

		public NetworkSynchronizer(IDataAccessService dataAccessService,
                             IHashCalculationsRepository hashCalculationsRepository,
                             IConfigurationService configurationService,
                             IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository,
                             IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
							 IAppConfig appConfig,
                             ILoggerService loggerService)
		{
			_dataAccessService = dataAccessService;
            _synchronizerConfiguration = configurationService.Get<ISynchronizerConfiguration>();
			_defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
			_blockParsersRepositoriesRepository = blockParsersRepositoriesRepository;
			_identityKeyProvidersRegistry = identityKeyProvidersRegistry;
            _appConfig = appConfig;
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
			_logger = loggerService.GetLogger(nameof(NetworkSynchronizer));

			PipeIn = new ActionBlock<TaskCompletionWrapper<PacketBase>>(p => SendPacket(p));

			_connectityCheckerAwaiters = new ConcurrentDictionary<int, TaskCompletionSource<bool>>();
		}

		#region ============ PUBLIC FUNCTIONS =============  

		public DateTime LastSyncTime { get; set; }
		
		public IPropagatorBlock<WitnessPackage, WitnessPackage> PipeOut { get; set; }

		public ITargetBlock<TaskCompletionWrapper<PacketBase>> PipeIn { get; }

		public async Task<RegistryCombinedBlockModel> GetLastRegistryCombinedBlock() => await Task.FromResult(_lastCombinedBlockDescriptor).ConfigureAwait(false);

		public void SendPacket(TaskCompletionWrapper<PacketBase> wrapper)
        {
			_logger.LogIfDebug(() => $"Sending to Node {_synchronizerConfiguration.NodeApiUri} packet {wrapper.State.GetType().Name}");

            try
            {
				_synchronizerConfiguration.NodeApiUri
					.AppendPathSegment("Packet")
					.PostJsonAsync(wrapper.State)
					.ContinueWith(async (t, o) => 
					{
						var w = o as TaskCompletionWrapper<PacketBase>;
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


		public async Task<SyncBlockModel> GetLastSyncBlock()
		{
			return await Task.FromResult(_lastSyncDescriptor).ConfigureAwait(false);
		}

		public async Task ProcessRtPackage(RtPackage rtPackage)
		{
			_logger.LogIfDebug(() => $"[N2G]: {nameof(ProcessRtPackage)}, rtPackage: {JsonConvert.SerializeObject(rtPackage, new ByteArrayJsonConverter())}");

			if ((_lastSyncDescriptor?.Height ?? 0) < rtPackage.CombinedBlock.SyncBlockHeight)
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

			SynchronizationRegistryCombinedBlock combinedBlock = ExtractRegistryCombinedBlock(rtPackage.CombinedBlock);
			RegistryFullBlock registryFullBlock = ExtractRegistryFullBlock(rtPackage.RegistryFullBlock);

			try
			{
				List<Task<WitnessPacket>> packetWitnessTasks = new List<Task<WitnessPacket>>();

				ProcessRegistryFullBlock(combinedBlock, registryFullBlock, packetWitnessTasks);

				SetWitnessTasks(combinedBlock, packetWitnessTasks);

				StoreRegistryCombinedBlock(combinedBlock);
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure during obtaining transactions at Registry Combined Block with height {combinedBlock.BlockHeight}", ex);
			}
		}

		public async Task SendEphemeralPacket(StealthBase packet)
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

			if (_dataAccessService.GetLastRegistryCombinedBlock(out ulong combinedBlockHeight, out byte[] combinedBlockContent))
			{
				_lastCombinedBlockDescriptor = new RegistryCombinedBlockModel(combinedBlockHeight, combinedBlockContent, _defaultHashCalculation.CalculateHash(combinedBlockContent));
			}
			else
			{
				_lastCombinedBlockDescriptor = new RegistryCombinedBlockModel(0, null, new byte[32]);
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
				SyncBlockModel syncBlockModel = await _synchronizerConfiguration.NodeApiUri.AppendPathSegment("GetLastSyncBlock").GetJsonAsync<SyncBlockModel>().ConfigureAwait(false);
				if (syncBlockModel != null)
				{
					_lastSyncDescriptor = syncBlockModel;
					_dataAccessService.UpdateLastSyncBlock(_lastSyncDescriptor.Height, _lastSyncDescriptor.Hash);
				}
				else
				{
					_lastSyncDescriptor = new SyncBlockModel(0, new byte[32]);
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
				RegistryCombinedBlockModel registryCombinedBlock = await _synchronizerConfiguration.NodeApiUri.AppendPathSegment("LastRegistryCombinedBlock").GetJsonAsync<RegistryCombinedBlockModel>().ConfigureAwait(false);
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

		private SynchronizationRegistryCombinedBlock ExtractRegistryCombinedBlock(TransactionInfo transactionInfo)
		{
			try
			{
				SynchronizationRegistryCombinedBlock combinedBlock;
				IBlockParsersRepository blockParsersRepository = _blockParsersRepositoriesRepository.GetBlockParsersRepository(transactionInfo.PacketType);
				IBlockParser blockParser = blockParsersRepository.GetInstance(transactionInfo.BlockType);
				PacketBase blockBase = blockParser.Parse(transactionInfo.Content);
				combinedBlock = (SynchronizationRegistryCombinedBlock)blockBase;
				return combinedBlock;
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure during deserealization of block of PacketType = {(PacketType)transactionInfo.PacketType} and  BlockType = {transactionInfo.BlockType} at Sync Block Height {transactionInfo.SyncBlockHeight}", ex);
			}

			return null;
		}

		private RegistryFullBlock ExtractRegistryFullBlock(TransactionInfo transactionInfo)
		{
			try
			{
				RegistryFullBlock packet;
				IBlockParsersRepository blockParsersRepository = _blockParsersRepositoriesRepository.GetBlockParsersRepository(transactionInfo.PacketType);
				IBlockParser blockParser = blockParsersRepository.GetInstance(transactionInfo.BlockType);
				PacketBase blockBase = blockParser.Parse(transactionInfo.Content);
				packet = (RegistryFullBlock)blockBase;
				return packet;
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure during deserealization of block of PacketType = {(PacketType)transactionInfo.PacketType} and  BlockType = {transactionInfo.BlockType} at Sync Block Height {transactionInfo.SyncBlockHeight}", ex);
			}

			return null;
		}

        private void StoreRegistryCombinedBlock(SynchronizationRegistryCombinedBlock combinedBlock)
        {
            _dataAccessService.StoreRegistryCombinedBlock(combinedBlock.BlockHeight, combinedBlock.RawData.ToArray());

            if ((_lastCombinedBlockDescriptor?.Height ?? 0) < combinedBlock.BlockHeight)
            {
				if(_lastCombinedBlockDescriptor == null)
                {
					_lastCombinedBlockDescriptor = new RegistryCombinedBlockModel(0, null, new byte[32]);
				}
                _lastCombinedBlockDescriptor.Height = combinedBlock.BlockHeight;
            }
        }

        private void SetWitnessTasks(SynchronizationRegistryCombinedBlock combinedBlock, List<Task<WitnessPacket>> packetWitnessTasks)
        {
            if ((packetWitnessTasks?.Count ?? 0) > 0)
            {
				_logger.Info($"Waiting for storing {packetWitnessTasks?.Count} packets");
                Task.WhenAll(packetWitnessTasks).ContinueWith((t, o) =>
                {
                    if (t.IsCompletedSuccessfully)
                    {
						_logger.Info($"Packets were stored successfully");
						WitnessPackage witnessPackage = new WitnessPackage
                        {
                            StateWitnesses = t.Result.Where(w => w.ReferencedKeyImage == null).Select(w => new PacketWitness { WitnessId = w.WitnessPacketId, DestinationKey = w.ReferencedDestinationKey.HexStringToByteArray(), TransactionKey = w.ReferencedTransactionKey.HexStringToByteArray(), IsIdentityIssuing = ((PacketType)w.ReferencedPacketType == PacketType.Transactional && (w.ReferencedBlockType == ActionTypes.Transaction_IssueBlindedAsset || w.ReferencedBlockType == ActionTypes.Transaction_IssueAssociatedBlindedAsset || w.ReferencedBlockType == ActionTypes.Transaction_transferAssetToStealth)) }).ToArray(),
                            StealthWitnesses = t.Result.Where(w => w.ReferencedKeyImage != null).Select(w => new PacketWitness { WitnessId = w.WitnessPacketId, DestinationKey = w.ReferencedDestinationKey.HexStringToByteArray(), DestinationKey2 = w.ReferencedDestinationKey2.HexStringToByteArray(), KeyImage = w.ReferencedKeyImage.HexStringToByteArray(), TransactionKey = w.ReferencedTransactionKey.HexStringToByteArray() }).ToArray(),
                            CombinedBlockHeight = (ulong)o
                        };
						_logger.Info($"Sending witnesses[{witnessPackage.CombinedBlockHeight}]: State = {string.Join(",", witnessPackage.StateWitnesses?.Select(w => w.WitnessId))}, UTXO = {string.Join(",", witnessPackage.StealthWitnesses?.Select(w => w.WitnessId))}");
						PipeOut?.SendAsync(witnessPackage);
                    }
					else
					{
						_logger.Error($"Packets were not stored successfully", t.Exception);
					}
				}, combinedBlock.BlockHeight, TaskScheduler.Current);
            }
        }

        private void ProcessRegistryFullBlock(SynchronizationRegistryCombinedBlock combinedBlock, RegistryFullBlock registryFullBlock, List<Task<WitnessPacket>> packetWitnessTasks)
        {
            _logger.Info($"[N2G]: Obtained {registryFullBlock.StateWitnesses.Count()} StateWitnesses and {registryFullBlock.UtxoWitnesses.Count()} UtxoWitnesses");

            try
            {
                if (packetWitnessTasks == null)
                {
                    throw new ArgumentNullException(nameof(packetWitnessTasks));
                }

                if (registryFullBlock.StateWitnesses != null)
                {
                    foreach (var item in registryFullBlock.StateWitnesses)
                    {
                        TaskCompletionSource<WitnessPacket> taskCompletionSource = _dataAccessService.StoreWitnessPacket(registryFullBlock.SyncBlockHeight, (long)registryFullBlock.BlockHeight, combinedBlock.BlockHeight, (ushort)item.ReferencedPacketType, item.ReferencedBlockType, item.ReferencedBodyHash, item.ReferencedTarget, null, item.ReferencedTransactionKey, null);

                        TaskCompletionSource<WitnessPacket> taskCompletionSource2 = ObtainAndStoreStateTransaction(taskCompletionSource);

                        packetWitnessTasks.Add(taskCompletionSource2.Task);
                    }
                }

                if (registryFullBlock.UtxoWitnesses != null)
                {
                    foreach (var item in registryFullBlock.UtxoWitnesses)
                    {
                        TaskCompletionSource<WitnessPacket> taskCompletionSource = _dataAccessService.StoreWitnessPacket(registryFullBlock.SyncBlockHeight, (long)registryFullBlock.BlockHeight, combinedBlock.BlockHeight, (ushort)item.ReferencedPacketType, item.ReferencedBlockType, item.ReferencedBodyHash, item.DestinationKey, item.DestinationKey2, item.TransactionPublicKey, item.KeyImage.Value.ToArray());

                        TaskCompletionSource<WitnessPacket> taskCompletionSource2 = ObtainAndStoreUtxoTransaction(taskCompletionSource);

                        packetWitnessTasks.Add(taskCompletionSource2.Task);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error($"Failure at {nameof(ProcessRegistryFullBlock)}", ex);
                throw;
            }        
        }

        private TaskCompletionSource<WitnessPacket> ObtainAndStoreStateTransaction(TaskCompletionSource<WitnessPacket> taskCompletionSource)
		{
            TaskCompletionSource<WitnessPacket> taskCompletionSource2 = new TaskCompletionSource<WitnessPacket>();

            taskCompletionSource.Task.ContinueWith(async (t, o) =>
			{
				WitnessPacket witnessPacket = t.Result;
				TaskCompletionSource<WitnessPacket> completionSource = (TaskCompletionSource<WitnessPacket>)o;
				Url url = _synchronizerConfiguration.NodeApiUri.AppendPathSegment("GetTransactionInfoState").SetQueryParam("combinedBlockHeight", t.Result.CombinedBlockHeight).SetQueryParam("hash", t.Result.ReferencedBodyHash.Hash);
				_logger.Info($"Querying Transactional packet with URL {url}");
				
				try
				{
					Transactions.Core.DataModel.TransactionInfo transactionInfo = await url.GetJsonAsync<Transactions.Core.DataModel.TransactionInfo>().ConfigureAwait(false);
					_logger.LogIfDebug(() => $"Transactional packet obtained from URL {url}: {JsonConvert.SerializeObject(transactionInfo, new ByteArrayJsonConverter())}");
					
					if (transactionInfo.Content?.Any()??false)
					{
						StorePacket(witnessPacket, transactionInfo.Content);
					}
					else
					{
						_logger.Error($"Empty Transactional packet obtained with PacketType {transactionInfo.PacketType}, BlockType {transactionInfo.BlockType} at SyncBlockHeight {transactionInfo.SyncBlockHeight}");
					}

					completionSource.SetResult(witnessPacket);
				}
				catch (Exception ex)
				{
					completionSource.SetException(ex);
					_logger.Error($"Failure during obtaining and storing Transactional packet from URL {url}", ex);
				}
			}, taskCompletionSource2, TaskScheduler.Current);

            return taskCompletionSource2;
        }

        private TaskCompletionSource<WitnessPacket> ObtainAndStoreUtxoTransaction(TaskCompletionSource<WitnessPacket> taskCompletionSource)
        {
            TaskCompletionSource<WitnessPacket> taskCompletionSource2 = new TaskCompletionSource<WitnessPacket>();

            taskCompletionSource.Task.ContinueWith((t, o) =>
            {
				Url url = _synchronizerConfiguration.NodeApiUri.AppendPathSegment("GetTransactionInfoUtxo").SetQueryParam("combinedBlockHeight", t.Result.CombinedBlockHeight).SetQueryParam("hash", t.Result.ReferencedBodyHash.Hash);
				_logger.Info($"Querying UTXO packet with URL {url}");
				url.GetJsonAsync<Transactions.Core.DataModel.TransactionInfo>()
				.ContinueWith((t1, o2) =>
                {
					Tuple<WitnessPacket, TaskCompletionSource<WitnessPacket>> tuple = (Tuple<WitnessPacket, TaskCompletionSource<WitnessPacket>>)o2;
					if(t1.IsCompletedSuccessfully)
					{
						_logger.Info($"UTXO packet Packet Type {t1.Result.PacketType}, BlockType {t1.Result.BlockType} at SyncBlockHeight {t1.Result.SyncBlockHeight} with empty[{!(t1.Result.Content?.Any()??false)}] content obtained");

						if (t1.Result.Content?.Any()??false)
						{
							StorePacket(tuple.Item1, t1.Result.Content);
							tuple.Item2.SetResult(tuple.Item1);
						}
						else
						{
							_logger.Error("Empty UTXO packet obtained");
						}
					}
					else
					{
                        tuple.Item2.SetException(t1.Exception);

                        _logger.Error($"Failure during obtaining and storing UTXO packet", t1.Exception);
                        foreach (var ex in t1.Exception.InnerExceptions)
                        {
                            _logger.Error(ex.Message);
                        }
                    }
				}, new Tuple<WitnessPacket, TaskCompletionSource<WitnessPacket>>(t.Result, (TaskCompletionSource<WitnessPacket>)o), TaskScheduler.Current);
            }, taskCompletionSource2, TaskScheduler.Current);

            return taskCompletionSource2;
        }

		public IEnumerable<WitnessPackage> GetWitnessRange(ulong combinedBlockHeightStart, ulong combinedBlockHeightEnd = 0)
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
							CombinedBlockHeight = (ulong)key,
							StateWitnesses = witnessPackets[key].Where(w => w.ReferencedKeyImage == null).Select(w => new PacketWitness { WitnessId = w.WitnessPacketId, DestinationKey = w.ReferencedDestinationKey.HexStringToByteArray(), TransactionKey = w.ReferencedTransactionKey.HexStringToByteArray(), IsIdentityIssuing = ((PacketType)w.ReferencedPacketType == PacketType.Transactional && (w.ReferencedBlockType == ActionTypes.Transaction_IssueBlindedAsset || w.ReferencedBlockType == ActionTypes.Transaction_IssueAssociatedBlindedAsset || w.ReferencedBlockType == ActionTypes.Transaction_transferAssetToStealth)) }).ToArray(),
							StealthWitnesses = witnessPackets[key].Where(w => w.ReferencedKeyImage != null).Select(w => new PacketWitness { WitnessId = w.WitnessPacketId, DestinationKey = w.ReferencedDestinationKey.HexStringToByteArray(), DestinationKey2 = w.ReferencedDestinationKey2.HexStringToByteArray(), TransactionKey = w.ReferencedTransactionKey.HexStringToByteArray(), KeyImage = w.ReferencedKeyImage.HexStringToByteArray() }).ToArray()
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

		public async Task<IEnumerable<PacketInfo>> GetPacketInfos(IEnumerable<long> witnessIds)
		{
			_logger.Debug($"Getting packets for witnesses [{string.Join(',',witnessIds)}]");

			try
			{
				List<PacketInfo> packetInfos = new List<PacketInfo>();

				foreach (long witnessId in witnessIds)
				{
					_logger.Info($"Getting packet for witness with id {witnessId}");
					WitnessPacket witness = _dataAccessService.GetWitnessPacket(witnessId);
					_logger.LogIfDebug(() => $"{nameof(WitnessPacket)}: {JsonConvert.SerializeObject(witness, new ByteArrayJsonConverter())}");

					if (witness.ReferencedKeyImage != null)
					{
						StealthPacket utxoIncomingBlock = _dataAccessService.GetUtxoIncomingBlock(witnessId);

						if(utxoIncomingBlock == null)
						{
							utxoIncomingBlock = await RetryObtainUtxoPacket(witness).ConfigureAwait(false);
						}

						if (utxoIncomingBlock != null)
						{
							_logger.LogIfDebug(() => $"{nameof(StealthPacket)}: {JsonConvert.SerializeObject(utxoIncomingBlock, new ByteArrayJsonConverter())}");
							packetInfos.Add(new PacketInfo { PacketType = PacketType.Stealth, BlockType = utxoIncomingBlock.BlockType, Content = utxoIncomingBlock.Content });
						}
					}
					else
					{
						TransactionalPacket transactionalIncomingBlock = _dataAccessService.GetTransactionalIncomingBlock(witnessId);

						if(transactionalIncomingBlock == null)
						{
							transactionalIncomingBlock = await RetryObtainTransactionalPacket(witness).ConfigureAwait(false);
						}

						if(transactionalIncomingBlock != null)
						{
							_logger.LogIfDebug(() => $"{nameof(TransactionalPacket)}: {JsonConvert.SerializeObject(transactionalIncomingBlock, new ByteArrayJsonConverter())}");
							packetInfos.Add(new PacketInfo { PacketType = PacketType.Transactional, BlockType = transactionalIncomingBlock.BlockType, Content = transactionalIncomingBlock.Content });
						}
					}
				}

				return await Task.FromResult(packetInfos).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure at {nameof(GetPacketInfos)}([{string.Join(',', witnessIds)}])", ex);
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

		private async Task<StealthPacket> RetryObtainUtxoPacket(WitnessPacket witness)
		{
			_logger.Warning($"{nameof(StealthPacket)} for WitnessId {witness.WitnessPacketId} is missing. Retry to obtain");

			Url url = _synchronizerConfiguration.NodeApiUri.AppendPathSegment("GetTransactionInfoUtxo").SetQueryParam("combinedBlockHeight", witness.CombinedBlockHeight).SetQueryParam("hash", witness.ReferencedBodyHash.Hash);
			_logger.Info($"Querying UTXO packet with URL {url}");
			Transactions.Core.DataModel.TransactionInfo transactionInfo = await url.GetJsonAsync<Transactions.Core.DataModel.TransactionInfo>().ConfigureAwait(false);
			StorePacket(witness, transactionInfo.Content);
			StealthPacket utxoIncomingBlock = _dataAccessService.GetUtxoIncomingBlock(witness.WitnessPacketId);
			if (utxoIncomingBlock == null)
			{
				_logger.Error($"Failed retry to obtain {nameof(StealthPacket)} for WitnessId {witness.WitnessPacketId}");
			}

			return utxoIncomingBlock;
		}

		private async Task<TransactionalPacket> RetryObtainTransactionalPacket(WitnessPacket witness)
		{
			_logger.Warning($"{nameof(TransactionalPacket)} for WitnessId {witness.WitnessPacketId} is missing. Retry to obtain");

			Url url = _synchronizerConfiguration.NodeApiUri.AppendPathSegment("GetTransactionInfoState").SetQueryParam("combinedBlockHeight", witness.CombinedBlockHeight).SetQueryParam("hash", witness.ReferencedBodyHash.Hash);
			_logger.Info($"Querying Transactional packet with URL {url}");
            TransactionInfo transactionInfo = await url.GetJsonAsync<TransactionInfo>().ConfigureAwait(false);
			StorePacket(witness, transactionInfo.Content);
			TransactionalPacket transactionalIncomingBlock = _dataAccessService.GetTransactionalIncomingBlock(witness.WitnessPacketId);
			if (transactionalIncomingBlock == null)
			{
				_logger.Error($"Failed retry to obtain {nameof(TransactionalPacket)} for WitnessId {witness.WitnessPacketId}");
			}

			return transactionalIncomingBlock;
		}

		private void StorePacket(WitnessPacket wp, byte[] content)
		{
			StoreParsedPacket(wp, content);
		}

		private void StoreParsedPacket(WitnessPacket wp, byte[] content)
		{
			ulong registryCombinedBlockHeight = (ulong)wp.CombinedBlockHeight;
			PacketType packetType = (PacketType)wp.ReferencedPacketType;
			ushort blockType = wp.ReferencedBlockType;
			
			_logger.LogIfDebug(() => $"{nameof(StoreParsedPacket)} PacketType = {packetType}, BlockType = {blockType}, HashId={wp.ReferencedBodyHash.PacketHashId}, Hash={wp.ReferencedBodyHash.Hash}");

			PacketBase packet = GetParsedPacket(content, packetType, blockType);

			if (packet == null)
			{
				_logger.Error($"Failed to parse packet with PacketType = {packetType}, BlockType = {blockType}, HashId={wp.ReferencedBodyHash.PacketHashId}");
				return;
			}

			try
			{
				_logger.LogIfDebug(() => $"Storing packet {packet.GetType()} started");

				if (packet.PacketType == (ushort)PacketType.Transactional)
				{
					switch (packet.BlockType)
					{
						case ActionTypes.Transaction_IssueBlindedAsset:
							StoreIssueBlindedAsset(wp.WitnessPacketId, registryCombinedBlockHeight, (IssueBlindedAsset)packet);
							break;
						case ActionTypes.Transaction_IssueAssociatedBlindedAsset:
							StoreIssueAssociatedBlindedAsset(wp.WitnessPacketId, registryCombinedBlockHeight, (IssueAssociatedBlindedAsset)packet);
							break;
						case ActionTypes.Transaction_transferAssetToStealth:
							StoreTransferAsset(wp.WitnessPacketId, registryCombinedBlockHeight, (TransferAssetToStealth)packet);
							break;
						case ActionTypes.Transaction_EmployeeRecord:
							StoreEmployeeRecordPacket(wp.WitnessPacketId, registryCombinedBlockHeight, (EmployeeRecord)packet);
							break;
						case ActionTypes.Transaction_CancelEmployeeRecord:
							StoreCancelEmployeeRecordPacket(wp.WitnessPacketId, registryCombinedBlockHeight, (CancelEmployeeRecord)packet);
							break;
						case ActionTypes.Transaction_DocumentRecord:
							StoreDocumentRecord(wp.WitnessPacketId, registryCombinedBlockHeight, (DocumentRecord)packet);
							break;
						case ActionTypes.Transaction_DocumentSignRecord:
							StoreDocumentSignRecord(wp.WitnessPacketId, registryCombinedBlockHeight, (DocumentSignRecord)packet);
							break;
					}
				}
				else if (packet.PacketType == (ushort)PacketType.Stealth)
				{
					switch (packet.BlockType)
					{
						case ActionTypes.Stealth_TransitionCompromisedProofs:
							_dataAccessService.AddCompromisedKeyImage(((TransitionCompromisedProofs)packet).CompromisedKeyImage.ToHexString());
							break;
						case ActionTypes.Stealth_RevokeIdentity:
							ProcessRevokeIdentity((RevokeIdentity)packet, registryCombinedBlockHeight);
							break;
					}
					StoreUtxoPacket(wp.WitnessPacketId, registryCombinedBlockHeight, (StealthTransactionBase)packet);
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure during storing packet {packet.GetType()}", ex);
			}
			finally
			{
				_logger.LogIfDebug(() => $"Storing packet {packet.GetType()} finished");
			}
		}

		private PacketBase GetParsedPacket(byte[] content, PacketType packetType, ushort blockType)
		{
			PacketBase packet = null;
			try
			{
				IBlockParsersRepository blockParsersRepository = _blockParsersRepositoriesRepository.GetBlockParsersRepository(packetType);
				IBlockParser blockParser = blockParsersRepository?.GetInstance(blockType);
				packet = blockParser.Parse(content);
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed to parse packet of PacketType {packetType} and BlockType {blockType}", ex);
			}

			return packet;
		}

		private void StoreDocumentSignRecord(long witnessId, ulong registryCombinedBlockHeight, DocumentSignRecord packet)
		{
			StateIncomingStoreInput storeInput = new StateIncomingStoreInput
			{
				SyncBlockHeight = packet.SyncBlockHeight,
				CombinedRegistryBlockHeight = registryCombinedBlockHeight,
				WitnessId = witnessId,
				BlockHeight = packet.BlockHeight,
				BlockType = packet.BlockType,
				Commitment = packet.SignerCommitment,
				Destination = null,
				Source = packet.Signer.Value.ToArray(),
				Content = packet.RawData.ToArray()
			};

			_dataAccessService.StoreIncomingTransactionalBlock(storeInput, null);
		}

        private void StoreDocumentRecord(long witnessId, ulong registryCombinedBlockHeight, DocumentRecord packet)
        {
            StateIncomingStoreInput storeInput = new StateIncomingStoreInput
            {
                SyncBlockHeight = packet.SyncBlockHeight,
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                BlockHeight = packet.BlockHeight,
                BlockType = packet.BlockType,
                Commitment = packet.DocumentHash,
                Destination = null,
                Source = packet.Signer.Value.ToArray(),
                Content = packet.RawData.ToArray()
            };

            _dataAccessService.StoreIncomingTransactionalBlock(storeInput, null);
        }

        private void StoreCancelEmployeeRecordPacket(long witnessId, ulong registryCombinedBlockHeight, CancelEmployeeRecord packet)
        {
            StateIncomingStoreInput storeInput = new StateIncomingStoreInput
            {
                SyncBlockHeight = packet.SyncBlockHeight,
                CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
                BlockHeight = packet.BlockHeight,
                BlockType = packet.BlockType,
                Commitment = packet.RegistrationCommitment,
                Destination = null,
                Source = packet.Signer.Value.ToArray(),
                Content = packet.RawData.ToArray()
            };

            _dataAccessService.StoreIncomingTransactionalBlock(storeInput, null);
            _dataAccessService.CancelEmployeeRecord(packet.Signer.Value, packet.RegistrationCommitment);
        }

        private void StoreEmployeeRecordPacket(long witnessId, ulong registryCombinedBlockHeight, EmployeeRecord packet)
		{
			StateIncomingStoreInput storeInput = new StateIncomingStoreInput
			{
				SyncBlockHeight = packet.SyncBlockHeight,
				CombinedRegistryBlockHeight = registryCombinedBlockHeight,
				WitnessId = witnessId,
				BlockHeight = packet.BlockHeight,
				BlockType = packet.BlockType,
				Commitment = packet.RegistrationCommitment,
				Destination = null,
				Source = packet.Signer.Value.ToArray(),
				Content = packet.RawData.ToArray()
			};

			_dataAccessService.StoreIncomingTransactionalBlock(storeInput, packet.GroupCommitment);
			_dataAccessService.AddEmployeeRecord(packet.Signer.Value, packet.RegistrationCommitment, packet.GroupCommitment);
		}

		private void StoreUtxoPacket(long witnessId, ulong registryCombinedBlockHeight, StealthTransactionBase packet)
		{
			UtxoIncomingStoreInput storeInput = new UtxoIncomingStoreInput
			{
				SyncBlockHeight = packet.SyncBlockHeight,
				CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
				BlockType = packet.BlockType,
				Commitment = packet.AssetCommitment,
				Destination = packet.DestinationKey,
				DestinationKey2 = packet.DestinationKey2,
				KeyImage = packet.KeyImage.Value.ToArray(),
				TransactionKey = packet.TransactionPublicKey,
				Content = packet.RawData.ToArray()
			};

			_dataAccessService.StoreIncomingUtxoTransactionBlock(storeInput);
		}

		private void StoreIssueAssociatedBlindedAsset(long witnessId, ulong registryCombinedBlockHeight, IssueAssociatedBlindedAsset packet)
		{
			StateIncomingStoreInput storeInput = new StateIncomingStoreInput
			{
				SyncBlockHeight = packet.SyncBlockHeight,
				CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
				BlockHeight = packet.BlockHeight,
                BlockType = packet.BlockType,
				Commitment = packet.AssetCommitment,
				Destination = null,
				Source = packet.Signer.Value.ToArray(),
				Content = packet.RawData.ToArray()
			};

			_dataAccessService.StoreIncomingTransactionalBlock(storeInput, packet.GroupId);

			_dataAccessService.StoreAssociatedAttributeIssuance(packet.Signer.Value, packet.AssetCommitment, packet.RootAssetCommitment);
		}

		private void StoreIssueBlindedAsset(long witnessId, ulong registryCombinedBlockHeight, IssueBlindedAsset packet)
		{
			StateIncomingStoreInput storeInput = new StateIncomingStoreInput
			{
				SyncBlockHeight = packet.SyncBlockHeight,
				CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
				BlockHeight = packet.BlockHeight,
				BlockType = packet.BlockType,
				Commitment = packet.AssetCommitment,
				Destination = null,
				Source = packet.Signer.Value.ToArray(),
				Content = packet.RawData.ToArray()
			};

			_dataAccessService.StoreIncomingTransactionalBlock(storeInput, packet.GroupId);
		}

		private void ProcessRevokeIdentity(RevokeIdentity packet, ulong registryCombinedBlockHeight)
		{
			_dataAccessService.SetRootAttributesOverriden(packet.DestinationKey2, packet.EligibilityProof.AssetCommitments[0], (long)registryCombinedBlockHeight);
		}

		private void StoreTransferAsset(long witnessId, ulong registryCombinedBlockHeight, TransferAssetToStealth packet)
		{
			StateTransitionIncomingStoreInput storeInput = new StateTransitionIncomingStoreInput
			{
				SyncBlockHeight = packet.SyncBlockHeight,
				CombinedRegistryBlockHeight = registryCombinedBlockHeight,
                WitnessId = witnessId,
				BlockHeight = packet.BlockHeight,
				BlockType = packet.BlockType,
				Commitment = packet.TransferredAsset.AssetCommitment,
				Destination = packet.DestinationKey,
				TransactionKey = packet.TransactionPublicKey,
				Source = packet.Signer.Value.ToArray(),
				Content = packet.RawData.ToArray()
			};

			_dataAccessService.StoreIncomingTransitionTransactionalBlock(storeInput, null, packet.SurjectionProof.AssetCommitments[0]);
			_dataAccessService.SetRootAttributesOverriden(packet.Signer.Value, packet.SurjectionProof.AssetCommitments[0], (long)registryCombinedBlockHeight);
			_dataAccessService.StoreRootAttributeIssuance(packet.Signer.Value, packet.SurjectionProof.AssetCommitments[0], packet.TransferredAsset.AssetCommitment, (long)registryCombinedBlockHeight);
		}

		#endregion
	}
}
