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
using O10.Transactions.Core.Ledgers;

namespace O10.Node.Core.Centralized
{
    [RegisterDefaultImplementation(typeof(IRealTimeRegistryService), Lifetime = LifetimeManagement.Scoped)]
    public class RealTimeRegistryService : IRealTimeRegistryService
    {
		private readonly IIdentityKeyProvider _identityKeyProvider;
		private readonly BlockingCollection<Tuple<SynchronizationPacket, RegistryPacket>> _registrationPackets = new BlockingCollection<Tuple<SynchronizationPacket, RegistryPacket>>();
		private readonly ConcurrentDictionary<IKey, TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>>> _packetTriggers = new ConcurrentDictionary<IKey, TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>>>(new KeyEqualityComparer());
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

        public async Task PostPackets(SynchronizationPacket aggregatedRegistrationsPacket, RegistryPacket registrationsPacket, CancellationToken cancellationToken)
        {
			var registryTransaction = registrationsPacket.Transaction<FullRegistryTransaction>();

			_logger.Debug($"Received combined and registryFullBlock with {registryTransaction.Witnesses.Length} Witnesses. Wait for transaction packets...");

			foreach (var witness in registryTransaction.Witnesses)
			{
				if(witness.Payload.Transaction is RegisterTransaction transaction && _chainDataServices.Any(c => c.LedgerType == transaction.ReferencedLedgerType))
                {
                    var hashString = transaction.Parameters[RegisterTransaction.REFERENCED_BODY_HASH];
                    var hash = hashString.HexStringToByteArray();
                    TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>> taskCompletionSource = new TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>>();
                    _logger.LogIfDebug(() => $"Adding Witness TaskCompletionSource by hash {hashString}");
                    taskCompletionSource = _packetTriggers.GetOrAdd(_identityKeyProvider.GetKey(hash), taskCompletionSource);

                    await WaitForPacketStoredAndUpdateIt(aggregatedRegistrationsPacket, taskCompletionSource, cancellationToken);
                }
            }

			_registrationPackets.Add(new Tuple<SynchronizationPacket, RegistryPacket>(aggregatedRegistrationsPacket, registrationsPacket));

			if (_lowestCombinedBlockHeight > aggregatedRegistrationsPacket.Payload.Height)
			{
				_lowestCombinedBlockHeight = aggregatedRegistrationsPacket.Payload.Height;
			}
        }

        private async Task WaitForPacketStoredAndUpdateIt(SynchronizationPacket aggregatedRegistrationsPacket, TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>> taskCompletionSource, CancellationToken cancellationToken)
        {
            try
            {
                var res = await taskCompletionSource.Task;
                await _chainDataServices
                    .First(s => s.LedgerType == res.Value.LedgerType)
                    .AddDataKey(res.Key, new CombinedHashKey(aggregatedRegistrationsPacket.Payload.Height, res.Key.HashKey), cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to update the height of {nameof(AggregatedRegistrationsTransaction)} due to an error", ex);
            }
        }

        public void PostTransaction(TaskCompletionWrapper<IPacketBase> completionWrapper)
		{
            if (completionWrapper is null)
            {
                throw new ArgumentNullException(nameof(completionWrapper));
            }

            _logger.LogIfDebug(() => $"Posted packet {completionWrapper.State.GetType().Name}");

            completionWrapper.TaskCompletion.Task.ContinueWith((t, o) => 
            {
                var packet = (IPacketBase)o;
                if (t.IsCompletedSuccessfully)
                {
                    if(t.Result is ItemAddedNotification notification && notification.DataKey is HashAndIdKey hashAndIdKey)
                    {
                        _logger.LogIfDebug(() => $"Continue with a packet {packet.GetType().Name} with hash {hashAndIdKey.HashKey}");
                        TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>> taskCompletionSource = new TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>>();
                        taskCompletionSource = _packetTriggers.GetOrAdd(hashAndIdKey.HashKey, taskCompletionSource);
                        taskCompletionSource.SetResult(new KeyValuePair<HashAndIdKey, IPacketBase>(hashAndIdKey, packet));
                    }
                    else
                    {
                        _logger.Error($"Failed to proceed with a packet {packet.GetType().Name} due to an error", t.Exception.InnerException);
                    }
                }
                else
                {
                    _logger.Error($"Failed to proceed with registration of the packet {packet.GetType().Name} due to the error {t.Exception.InnerException.Message}", t.Exception.InnerException);
                }
            }, completionWrapper.State, TaskScheduler.Default);
		}
	}
}
