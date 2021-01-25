using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.DataModel;
using O10.Transactions.Core.Parsers;
using O10.Transactions.Core.Serializers;
using O10.Client.Common.Communication.SynchronizerNotifications;
using O10.Client.Common.Interfaces;
using O10.Core;
using O10.Core.Communication;
using O10.Core.Cryptography;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;

namespace O10.Client.Common.Communication
{
	public abstract class TransactionsServiceBase : ITransactionsService
	{
		protected readonly ISerializersFactory _serializersFactory;
		protected readonly IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
		protected readonly IGatewayService _gatewayService;
		protected readonly IHashCalculation _hashCalculation;
		protected readonly IHashCalculation _proofOfWorkCalculation;
		protected readonly IIdentityKeyProvider _identityKeyProvider;
		private readonly IPropagatorBlock<PacketBase, PacketBase> _pipeOutTransactions;
		protected ISigningService _signingService;
		protected readonly List<IObserver<SynchronizerNotificationBase>> _observers;
		protected readonly ILogger _logger;
		protected long _accountId;

		protected TransactionsServiceBase(
            IHashCalculationsRepository hashCalculationsRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ISerializersFactory serializersFactory,
            IBlockParsersRepositoriesRepository blockParsersRepositoriesRepository,
			ISigningService signingService,
			IGatewayService gatewayService,
            ILoggerService loggerService)
		{
			_serializersFactory = serializersFactory;
			_blockParsersRepositoriesRepository = blockParsersRepositoriesRepository;
			_hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
			_proofOfWorkCalculation = hashCalculationsRepository.Create(Globals.POW_TYPE);
			_identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
			_observers = new List<IObserver<SynchronizerNotificationBase>>();
			_pipeOutTransactions = new TransformBlock<PacketBase, PacketBase>(w => w);
			_signingService = signingService;
			_gatewayService = gatewayService;
			_logger = loggerService.GetLogger(GetType().Name);
		}

		protected async Task<bool> PropagateTransaction(PacketBase packet)
        {
			var res = await _pipeOutTransactions.SendAsync(packet).ConfigureAwait(false);
			
			return res;
		}

		protected byte[] GetPowHash(byte[] hash, ulong nonce)
		{
			BigInteger bigInteger = new BigInteger(hash);
			bigInteger += nonce;
			byte[] hashNonce = bigInteger.ToByteArray();
			byte[] powHash = _proofOfWorkCalculation.CalculateHash(hashNonce);
			return powHash;
		}

		protected virtual void FillAndSign(PacketBase packet, object signingArgs = null)
		{
			ISerializer serializer = _serializersFactory.Create(packet);
			serializer.SerializeBody();
			_signingService.Sign(packet, signingArgs);
			serializer.SerializeFully();

			_logger.LogIfDebug(() => $"[{_accountId}]: Sending packet {packet.GetType().Name}: {JsonConvert.SerializeObject(packet, new ByteArrayJsonConverter())}");
		}

		protected void FillSyncData(PacketBase packet)
		{
			SyncBlockModel lastSyncBlock = AsyncUtil.RunSync(() => _gatewayService.GetLastSyncBlock());
			packet.SyncBlockHeight = lastSyncBlock?.Height ?? 0;
			packet.PowHash = GetPowHash(lastSyncBlock?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE], 0);
		}

		public virtual ISourceBlock<T> GetSourcePipe<T>(string name = null)
		{
			if (typeof(T) == typeof(PacketBase))
			{
				return (ISourceBlock<T>)_pipeOutTransactions;
			}

			throw new InvalidOperationException($"No source blocks are available for type {typeof(T).FullName}");
		}

		public virtual ITargetBlock<T> GetTargetPipe<T>(string name = null)
		{
			throw new InvalidOperationException($"No target blocks are available for type {typeof(T).FullName}");
		}

		private sealed class Subscription : IDisposable
		{
			private readonly TransactionsServiceBase _service;
			private IObserver<SynchronizerNotificationBase> _observer;

			public Subscription(TransactionsServiceBase service, IObserver<SynchronizerNotificationBase> observer)
			{
				_service = service;
				_observer = observer;
			}

			public void Dispose()
			{
				IObserver<SynchronizerNotificationBase> observer = _observer;
				if (null != observer)
				{
					lock (_service._observers)
					{
						_service._observers.Remove(observer);
					}
					_observer = null;
				}
			}
		}
	}
}
