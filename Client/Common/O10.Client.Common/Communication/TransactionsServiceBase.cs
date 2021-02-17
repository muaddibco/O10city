﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Parsers;
using O10.Transactions.Core.Serializers;
using O10.Client.Common.Interfaces;
using O10.Core;
using O10.Core.Cryptography;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Serialization;
using O10.Client.Common.Communication.Notifications;
using O10.Core.Notifications;
using O10.Transactions.Core.DTOs;

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
		private readonly IPropagatorBlock<TaskCompletionWrapper<PacketBase>, TaskCompletionWrapper<PacketBase>> _pipeOutTransactions;
		protected ISigningService _signingService;
		protected readonly List<IObserver<NotificationBase>> _observers;
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
			_observers = new List<IObserver<NotificationBase>>();
			_pipeOutTransactions = new TransformBlock<TaskCompletionWrapper<PacketBase>, TaskCompletionWrapper<PacketBase>>(w => w);
			_signingService = signingService;
			_gatewayService = gatewayService;
			_logger = loggerService.GetLogger(GetType().Name);
		}

		protected TaskCompletionSource<NotificationBase> PropagateTransaction(PacketBase packet)
        {
			var packetWrapper = new TaskCompletionWrapper<PacketBase>(packet);

			_pipeOutTransactions.SendAsync(packetWrapper);
			
			return packetWrapper.TaskCompletion;
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
			packet.SyncHeight = lastSyncBlock?.Height ?? 0;
			packet.PowHash = GetPowHash(lastSyncBlock?.Hash ?? new byte[Globals.DEFAULT_HASH_SIZE], 0);
		}

		public virtual ISourceBlock<T> GetSourcePipe<T>(string name = null)
		{
			if (typeof(T) == typeof(TaskCompletionWrapper<PacketBase>))
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
			private IObserver<NotificationBase> _observer;

			public Subscription(TransactionsServiceBase service, IObserver<NotificationBase> observer)
			{
				_service = service;
				_observer = observer;
			}

			public void Dispose()
			{
				IObserver<NotificationBase> observer = _observer;
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
