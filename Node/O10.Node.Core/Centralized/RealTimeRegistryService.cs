using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Node.DataLayer.DataServices;
using System.Linq;
using O10.Core.Models;
using O10.Node.DataLayer.DataServices.Notifications;
using O10.Node.DataLayer.DataServices.Keys;

namespace O10.Node.Core.Centralized
{
    [RegisterDefaultImplementation(typeof(IRealTimeRegistryService), Lifetime = LifetimeManagement.Singleton)]
    public class RealTimeRegistryService : IRealTimeRegistryService
    {
		private readonly IIdentityKeyProvider _identityKeyProvider;
		private readonly BlockingCollection<Tuple<SynchronizationPacket, RegistryPacket>> _registrationPackets = new BlockingCollection<Tuple<SynchronizationPacket, RegistryPacket>>();
		private readonly ConcurrentDictionary<IKey, TaskCompletionSource<TaskCompletionWrapper<IKey>>> _packetTriggers = new ConcurrentDictionary<IKey, TaskCompletionSource<TaskCompletionWrapper<IKey>>>(new KeyEqualityComparer());
		private readonly HashSet<IChainDataService> _chainDataServices = new HashSet<IChainDataService>();
		private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
		private long _lowestCombinedBlockHeight = long.MaxValue;

		public RealTimeRegistryService(IHashCalculationsRepository hashCalculationsRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ILoggerService loggerService)
		{
			_hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
			_identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _logger = loggerService.GetLogger(nameof(RealTimeRegistryService));
		}

		public void RegisterInternalChainDataService(IChainDataService chainDataService)
        {
			_chainDataServices.Add(chainDataService);
        }

		public long GetLowestCombinedBlockHeight() => _lowestCombinedBlockHeight;

		public IEnumerable<Tuple<SynchronizationPacket, RegistryPacket>> GetRegistryConsumingEnumerable(CancellationToken cancellationToken)
        {
            return _registrationPackets.GetConsumingEnumerable(cancellationToken);
        }

        public void PostPackets(SynchronizationPacket aggregatedRegistrationsPacket, RegistryPacket registrationsPacket)
        {
			var registryTransaction = registrationsPacket.With<FullRegistryTransaction>();

			_logger.Debug($"Received combined and registryFullBlock with {registryTransaction.Witnesses.Length} Witnesses. Wait for transaction packets...");

			foreach (var witness in registryTransaction.Witnesses)
			{
				if(witness.Body is RegisterTransaction transaction && _chainDataServices.Any(c => c.LedgerType == transaction.ReferencedLedgerType))
                {
                    var hashString = transaction.Parameters[RegisterTransaction.REFERENCED_BODY_HASH];
                    var hash = hashString.HexStringToByteArray();
                    TaskCompletionSource<TaskCompletionWrapper<IKey>> taskCompletionSource = new TaskCompletionSource<TaskCompletionWrapper<IKey>>();
                    _logger.LogIfDebug(() => $"Adding Witness TaskCompletionSource by hash {hashString}");
                    taskCompletionSource = _packetTriggers.GetOrAdd(_identityKeyProvider.GetKey(hash), taskCompletionSource);

                    WaitForPacketStoredAndUpdateIt(aggregatedRegistrationsPacket, taskCompletionSource);
                }
            }

			_registrationPackets.Add(new Tuple<SynchronizationPacket, RegistryPacket>(aggregatedRegistrationsPacket, registrationsPacket));

			if (_lowestCombinedBlockHeight > aggregatedRegistrationsPacket.With<AggregatedRegistrationsTransaction>().Height)
			{
				_lowestCombinedBlockHeight = aggregatedRegistrationsPacket.With<AggregatedRegistrationsTransaction>().Height;
			}
        }

        private void WaitForPacketStoredAndUpdateIt(SynchronizationPacket aggregatedRegistrationsPacket, TaskCompletionSource<TaskCompletionWrapper<IKey>> taskCompletionSource)
        {
            taskCompletionSource.Task.ContinueWith(async (t, o) =>
            {
                var notification = await t.Result.TaskCompletion.Task.ConfigureAwait(false);
                var syncPacket = o as SynchronizationPacket;
                if (notification is ItemAddedNotification itemAdded && itemAdded.DataKey is HashAndIdKey key)
                {
                    _chainDataServices.First(s => s.LedgerType == key.HashKey)
                        .AddDataKey(key, new CombinedHashKey(syncPacket.With<AggregatedRegistrationsTransaction>().Height, t.Result.State));        
                }
            }, aggregatedRegistrationsPacket, TaskScheduler.Default);
        }

        public void PostTransaction(TaskCompletionWrapper<IKey> completionWrapperByTransactionHashKey)
		{
            if (completionWrapperByTransactionHashKey is null)
            {
                throw new ArgumentNullException(nameof(completionWrapperByTransactionHashKey));
            }

            byte[] hash = _hashCalculation.CalculateHash(completionWrapperByTransactionHashKey.State.ToByteArray());

			_logger.LogIfDebug(() => $"Posted packet {completionWrapperByTransactionHashKey.State.GetType().Name} with hash {hash.ToHexString()}");

			TaskCompletionSource<TaskCompletionWrapper<IKey>> taskCompletionSource = new TaskCompletionSource<TaskCompletionWrapper<IKey>>();
			var hashKey = _identityKeyProvider.GetKey(hash);
			taskCompletionSource = _packetTriggers.GetOrAdd(hashKey, taskCompletionSource);
			taskCompletionSource.SetResult(completionWrapperByTransactionHashKey);
		}
	}
}
