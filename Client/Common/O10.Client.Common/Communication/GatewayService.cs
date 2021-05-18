using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Exceptions;
using O10.Client.Common.Interfaces;
using O10.Client.Common.Interfaces.Inputs;
using O10.Client.Common.Interfaces.Outputs;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Identity;
using O10.Core.Serialization;
using O10.Client.Common.Communication.Notifications;
using O10.Core.Notifications;
using O10.Transactions.Core.DTOs;
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Crypto.Models;

namespace O10.Client.Common.Communication
{
    [RegisterDefaultImplementation(typeof(IGatewayService), Lifetime = LifetimeManagement.Singleton)]
	public class GatewayService : IGatewayService
    {
        private string _gatewayUri;
		private readonly object _sync = new object();
		private bool _isInitialized;
		private readonly ILogger _logger;
		private readonly IRestClientService _restClientService;
		private readonly IPropagatorBlock<NotificationBase, NotificationBase> _propagatorBlockNotifications;

		public GatewayService(IRestClientService restClientService, ILoggerService loggerService)
        {
			_logger = loggerService.GetLogger(nameof(GatewayService));
			_restClientService = restClientService;
			_propagatorBlockNotifications = new TransformBlock<NotificationBase, NotificationBase>(p => p);
		}

		public ITargetBlock<TaskCompletionWrapper<IPacketBase>> PipeInTransactions { get; private set; }
		public ISourceBlock<NotificationBase> PipeOutNotifications => _propagatorBlockNotifications;

		public async Task<byte[][]> GetIssuanceCommitments(Memory<byte> issuer, int amount)
        {
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "GetIssuanceCommitments", issuer.ToHexString(), amount.ToString(CultureInfo.InvariantCulture));

			byte[][] issuanceCommitments = await _restClientService.Request(url).GetJsonAsync<byte[][]>().ConfigureAwait(false);

            return issuanceCommitments;
        }

		// TODO: This function will be obsolete - must be replaced by another one where the height of aggregated registration will be obtained using the hash of the account transaction
		public async Task<ulong> GetCombinedBlockByTransactionHash(byte[] accountPublicKey, byte[] transactionHash)
		{
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "GetCombinedBlockByTransactionHash", accountPublicKey.ToHexString(), transactionHash.ToHexString());
			try
			{
				ulong combinedBlockHeight = await _restClientService.Request(url).GetJsonAsync<ulong>().ConfigureAwait(false);
				return combinedBlockHeight;
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed request {url}", ex);
				throw;
			}
		}

		public async Task<AggregatedRegistrationsTransactionDTO> GetLastRegistryCombinedBlock()
		{
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "GetLastRegistryCombinedBlock");
			try
			{
				AggregatedRegistrationsTransactionDTO registryCombinedBlockDescriptor = await _restClientService.Request(url).GetJsonAsync<AggregatedRegistrationsTransactionDTO>().ConfigureAwait(false);

				return registryCombinedBlockDescriptor;

			}
			catch (Exception ex)
			{
				_logger.Error($"Failed request {url}", ex);
				throw new LastRegistryCombinedBlockFailedException(ex);
			}
		}

		public async Task<SyncInfoDTO> GetLastSyncBlock()
		{
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "GetLastSyncBlock");

			SyncInfoDTO registryCombinedBlockDescriptor = await _restClientService.Request(url).GetJsonAsync<SyncInfoDTO>().ConfigureAwait(false);

			return registryCombinedBlockDescriptor;
		}

		public async Task<OutputModel[]> GetOutputs(int amount)
        {
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "GetOutputs", amount.ToString(CultureInfo.InvariantCulture));

			OutputModel[] outputModels = await _restClientService.Request(url).GetJsonAsync<OutputModel[]>().ConfigureAwait(false);

            return outputModels;
        }

        public bool Initialize(string gatewayUri, CancellationToken cancellationToken)
        {
			if(_isInitialized)
			{
				return false;
			}

			lock(_sync)
			{
				if(_isInitialized)
				{
					return false;
				}

				_isInitialized = true;
			}

            _gatewayUri = gatewayUri;

			PipeInTransactions = new ActionBlock<TaskCompletionWrapper<IPacketBase>>(async p =>
			{
                try
                {
					_logger.Info($"Sending transaction {p.State.GetType().Name}");
					_logger.LogIfDebug(() => JsonConvert.SerializeObject(p.State, new ByteArrayJsonConverter()));

					var response = await _restClientService
						.Request(_gatewayUri).AppendPathSegments("api", "synchronization", "SendPacket")
						.PostJsonAsync(p.State)
						.ReceiveJson<SendDataResponse>().ConfigureAwait(false);

					if (!response.Status)
					{
						if (!string.IsNullOrEmpty(response.ExistingHash))
						{
							_logger.Error($"Failed to send transaction {p.State.GetType().Name} because key image {((StealthPacket)p.State).Payload.Transaction.KeyImage} was already witnessed");
							KeyImageCorruptedNotification keyImageCorrupted = new KeyImageCorruptedNotification
							{
								KeyImage = ((StealthPacket)p.State).Payload.Transaction.KeyImage.ToByteArray(),
								ExistingHash = response.ExistingHash.HexStringToByteArray()
							};
							await _propagatorBlockNotifications.SendAsync(keyImageCorrupted).ConfigureAwait(false);

							p.TaskCompletion.SetResult(keyImageCorrupted);
						}
						else
						{
							_logger.Error($"Failed to send transaction {p.State.GetType().Name} due to unknown error");
							p.TaskCompletion.SetResult(new FailedNotification());
						}
					}
					else
					{
						p.TaskCompletion.SetResult(new SucceededNotification());
					}
                }
                catch (FlurlHttpException ex)
                {
					p.TaskCompletion.SetException(ex);
					_logger.Error($"Failure during invoking {ex.Call.FlurlRequest.Url}, HTTP status: {ex.Call.HttpStatus}, duration: {ex.Call.Duration?.TotalMilliseconds??0} msec", ex);
                }
                catch (Exception ex)
                {
					p.TaskCompletion.SetException(ex);
					_logger.Error("Failure during request execution", ex);
				}
			}, new ExecutionDataflowBlockOptions { CancellationToken = cancellationToken });

			return true;
        }

		public async Task<IEnumerable<TransactionBase>> GetTransactions(IEnumerable<long> witnessIds)
		{
			_logger.Debug($"Getting packet infos for witnesses with Ids {string.Join(',', witnessIds)}");

			List<TransactionBase> res = null;
			try
			{
				Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "packets");
                foreach (var wid in witnessIds)
                {
					url.QueryParams.Add("wid", wid);
                }
				await (await _restClientService.Request(url)
					.GetAsync().ContinueWith(async t =>
					{
						if (t.IsCompletedSuccessfully)
						{
							res = await t.ReceiveJson<List<TransactionBase>>().ConfigureAwait(false);
						}
						else
						{
							_logger.Error($"Failed request {t.Result.RequestMessage.RequestUri} with content {await t.Result.RequestMessage.Content.ReadAsStringAsync().ConfigureAwait(false)}", t.Exception);
						}
					}, TaskScheduler.Current).ConfigureAwait(false)).ConfigureAwait(false);

				if (res != null)
				{
					_logger.LogIfDebug(() => $"Getting packet infos for witnesses with Ids {string.Join(',', witnessIds)} completed. {JsonConvert.SerializeObject(res, new ByteArrayJsonConverter())}");
				}
				else
				{
					_logger.Error($"Getting packet infos for witnesses with Ids {string.Join(',', witnessIds)} failed");
				}
			}
			catch (Exception ex)
			{
				_logger.Error($"Failure during obtaining packet infos for witness ids {string.Join(',', witnessIds)}", ex);
			}

            return res;
		}

		public async Task<bool> IsRootAttributeValid(Memory<byte> issuer, Memory<byte> commitment)
		{
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "IsRootAttributeValid", issuer.ToHexString(), commitment.ToHexString());
			bool res = false;
			await _restClientService.Request(url).GetAsync()
				.ContinueWith(t =>
				{
					if(t.IsCompletedSuccessfully)
					{
						res = true;
					}
					else
					{
						string response = AsyncUtil.RunSync(async () => await t.Result.Content.ReadAsStringAsync().ConfigureAwait(false));
						_logger.Error($"Request {url} failed with response {response}");
					}
				}, TaskScheduler.Current)
				.ConfigureAwait(false);

			return res;
		}

		public async Task<bool> AreRootAttributesValid(Memory<byte> issuer, IEnumerable<Memory<byte>> commitments)
		{
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "AreRootAttributesValid", issuer.ToHexString());
			bool res = false;
			await _restClientService.Request(url)
				.PostJsonAsync(commitments.Select(a => a.ToHexString()))
				.ContinueWith(t =>
				{
					if (t.IsCompletedSuccessfully)
					{
						res = true;
					}
					else
					{
						string response = AsyncUtil.RunSync(async () => await t.Result.Content.ReadAsStringAsync().ConfigureAwait(false));
						_logger.Error($"Request {url} failed with response {response}");
					}
				}, TaskScheduler.Current)
				.ConfigureAwait(false);

			return res;
		}

		public async Task<bool> AreAssociatedAttributesExist(Memory<byte> issuer, (Memory<byte> issuanceCommitment, Memory<byte> commitmenttoRoot)[] attrs)
		{
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "AreAssociatedAttributesExist", issuer.ToHexString());
			bool res = false;
			await _restClientService.Request(url)
				.PostJsonAsync(attrs.ToDictionary(a => a.issuanceCommitment.ToHexString(), a => a.commitmenttoRoot.ToHexString()))
				.ContinueWith(t =>
				{
					if (t.IsCompletedSuccessfully)
					{
						res = true;
					}
					else
					{
						string response = AsyncUtil.RunSync(async () => await t.Result.Content.ReadAsStringAsync().ConfigureAwait(false));
						_logger.Error($"Request {url} failed with response {response}");
					}
				}, TaskScheduler.Current)
				.ConfigureAwait(false);

			return res;
		}

		public async Task<bool> WasRootAttributeValid(IKey issuer, Memory<byte> commitment, long combinedBlockHeight)
		{
            if (issuer is null)
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "WasRootAttributeValid", issuer.ToString(), commitment.ToHexString(), combinedBlockHeight);
			bool res = false;
			await _restClientService.Request(url).GetJsonAsync<bool>()
				.ContinueWith(t =>
				{
					if (t.IsCompletedSuccessfully)
					{
						res = t.Result;
					}
					else
					{
						_logger.Error($"Request {url} failed");
					}
				}, TaskScheduler.Current)
				.ConfigureAwait(false);

			return res;
		}

		public async Task<IEnumerable<WitnessPackage>> GetWitnessesRange(long rangeStart, long rangeEnd)
		{
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "GetWitnessesRange", rangeStart, rangeEnd);

			try
			{
				IEnumerable<WitnessPackage> witnessPackages =
					await _restClientService.Request(url)
					.ConfigureRequest(s => { s.Timeout = TimeSpan.FromMinutes(60); })
					.GetJsonAsync<IEnumerable<WitnessPackage>>().ConfigureAwait(false);

				return witnessPackages;
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed request {url.ToString()}", ex);
				return null;
			}
		}

        public string GetNotificationsHubUri()
        {
            return _gatewayUri.AppendPathSegments("notificationsHub");
        }

		public async Task<byte[]> GetEmployeeRecordGroup(byte[] issuer, byte[] registrationCommitment)
		{
			byte[] res = null;

			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "GetEmployeeRecordGroup", issuer.ToHexString(), registrationCommitment.ToHexString());
			await _restClientService.Request(url).GetStringAsync()
				.ContinueWith(t =>
				{
					if(t.IsCompletedSuccessfully)
					{
						_logger.Debug($"Request {url} returned {t.Result ?? "NULL"}");
						res = t.Result?.HexStringToByteArray();
					}
					else
					{
						_logger.Error($"Failed request {url}", t.Exception);
					}
				}, TaskScheduler.Current)
				.ConfigureAwait(false);

			return res;
		}

		public async Task<TransactionBase> GetTransaction(string source, byte[] transactionHash)
		{
            Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "Transaction");
            url.QueryParams.Add("source", source);
            url.QueryParams.Add("transactionHash", transactionHash.ToHexString());

            var res = await _restClientService.Request(url).GetJsonAsync<TransactionBase>().ConfigureAwait(false);

			return res;
		}

        public async Task<string> PushRelationProofSession(RelationProofsData relationProofSession)
        {
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "PushRelationProofSession");

			RelationProofSessionResponse response = await _restClientService.Request(url).PostJsonAsync(relationProofSession).ReceiveJson<RelationProofSessionResponse>().ConfigureAwait(false);

            return response.SessionKey;
        }

        public async Task<RelationProofsData> PopRelationProofSession(string sessionKey)
        {
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "PopRelationProofSession", sessionKey);

			return await _restClientService.Request(url).GetJsonAsync<RelationProofsData>().ConfigureAwait(false);
        }

		public async Task<byte[]> GetHashByKeyImage(byte[] keyImage)
		{
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "HashByKeyImage", keyImage.ToHexString());
			string hashString = await _restClientService.Request(url).GetStringAsync().ConfigureAwait(false);

			return hashString?.HexStringToByteArray();
		}

        public async Task<bool> IsKeyImageCompromised(IKey keyImage)
        {
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "IsKeyImageCompomised").SetQueryParam("keyImage", keyImage);

			string res = await _restClientService.Request(url).GetStringAsync().ConfigureAwait(false);

            return bool.Parse(res);
        }

		public async Task<IEnumerable<InfoMessage>> GetInfo()
        {
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization");
			return await _restClientService.Request(url).GetJsonAsync<IEnumerable<InfoMessage>>().ConfigureAwait(false);
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
			Url url = _gatewayUri.AppendPathSegments("api", "synchronization", "GetLastPacketInfo", accountPublicKey);
			try
			{
				var packetInfo = await _restClientService.Request(url).GetJsonAsync<StatePacketInfo>().ConfigureAwait(false);

				return packetInfo;
			}
			catch (Exception ex)
			{
				_logger.Error($"Failed request {url}", ex);
				throw;
			}
		}
	}
}
