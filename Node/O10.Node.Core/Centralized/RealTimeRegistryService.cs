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
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Transactions.Core.Ledgers;
using O10.Network.Handlers;

namespace O10.Node.Core.Centralized
{
    [RegisterDefaultImplementation(typeof(IRealTimeRegistryService), Lifetime = LifetimeManagement.Scoped)]
    public class RealTimeRegistryService : IRealTimeRegistryService, IDisposable
    {
		private readonly IIdentityKeyProvider _identityKeyProvider;
		private readonly BlockingCollection<Tuple<SynchronizationPacket, RegistryPacket>> _registrationPackets = new BlockingCollection<Tuple<SynchronizationPacket, RegistryPacket>>();
		private static readonly ConcurrentDictionary<IKey, TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>>> _packetTriggers = new ConcurrentDictionary<IKey, TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>>>(new KeyEqualityComparer());
		private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
        private readonly IChainDataServicesRepository _chainDataServicesRepository;
        private long _lowestCombinedBlockHeight = long.MaxValue;
        private bool _disposedValue;

        public RealTimeRegistryService(
            IHashCalculationsRepository hashCalculationsRepository,
            IChainDataServicesRepository chainDataServicesRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            IHandlingFlowContext handlingFlowContext,
            ILoggerService loggerService)
		{
			_hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
			_identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _logger = loggerService.GetLogger($"{nameof(RealTimeRegistryService)}#{handlingFlowContext.Index}");
            _logger.Debug(() => $"Creating {nameof(RealTimeRegistryService)}");
            _chainDataServicesRepository = chainDataServicesRepository;
        }

		public long GetLowestCombinedBlockHeight() => _lowestCombinedBlockHeight;

		public IEnumerable<Tuple<SynchronizationPacket, RegistryPacket>> GetRegistryConsumingEnumerable(CancellationToken cancellationToken)
        {
            return _registrationPackets.GetConsumingEnumerable(cancellationToken);
        }

        public async Task PostPackets(SynchronizationPacket aggregatedRegistrationsPacket, RegistryPacket registrationsPacket, CancellationToken cancellationToken)
        {
			var registryTransaction = registrationsPacket.Transaction<FullRegistryTransaction>();

			_logger.Debug(() => $"Received combined and registryFullBlock with {registryTransaction.Witnesses.Length} Witnesses. Going to wait for transactions themselves...");

			foreach (var witness in registryTransaction.Witnesses)
			{
                try
                {
                    if (!(witness.Payload.Transaction is RegisterTransaction transaction))
                    {
                        _logger.Error($"Obtained witness payload is '{witness.Payload.Transaction.GetType().FullName}' while it was expected '{typeof(RegisterTransaction).FullName}' only!");
                        continue;
                    }

                    _logger.Debug(() => $"Processing witness for the transaction of type '{transaction.ReferencedLedgerType}' and with hash {transaction.Parameters[RegisterTransaction.TRANSACTION_HASH]}");

                    IChainDataService chainDataService;
                    if ((chainDataService = _chainDataServicesRepository.GetInstance(transaction.ReferencedLedgerType)) != null)
                    {
                        var hashString = transaction.Parameters[RegisterTransaction.TRANSACTION_HASH];
                        var hash = hashString.HexStringToByteArray();
                        var taskCompletionSource = _packetTriggers.GetOrAdd(_identityKeyProvider.GetKey(hash), new TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>>());

                        _logger.Debug(() => $"Waiting for storing the packet with hash {hashString}");
                        var res = await taskCompletionSource.Task;
                        _logger.Debug(() => $"Packet with hash {hashString} stored");
                        await chainDataService.AddDataKey(res.Key, new CombinedHashKey(aggregatedRegistrationsPacket.Payload.Height, res.Key.HashKey), cancellationToken);
                    }
                    else
                    {
                        _logger.Error($"No data service found for the transaction of type '{transaction.ReferencedLedgerType}'!");
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error($"Failure during processing transaction witness\r\n{witness.ToJson()}", ex);
                }            
            }

			_registrationPackets.Add(new Tuple<SynchronizationPacket, RegistryPacket>(aggregatedRegistrationsPacket, registrationsPacket), cancellationToken);

			if (_lowestCombinedBlockHeight > aggregatedRegistrationsPacket.Payload.Height)
			{
				_lowestCombinedBlockHeight = aggregatedRegistrationsPacket.Payload.Height;
			}
        }

        public void PostTransaction(DataResult<IPacketBase> result)
        {
            if (result is null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (!(result.Key is HashAndIdKey hashAndIdKey))
            {
                throw new ArgumentException($"Key must be of the type {typeof(HashAndIdKey).Name}");
            }

            _logger.Debug(() => $"Posted packet {result.Packet.GetType().Name}");

            var packet = result.Packet;
            _logger.Debug(() => $"Releasing trigger of the packet with hash {hashAndIdKey.HashKey}");
            var taskCompletionSource = _packetTriggers.GetOrAdd(hashAndIdKey.HashKey, new TaskCompletionSource<KeyValuePair<HashAndIdKey, IPacketBase>>());
            taskCompletionSource.SetResult(new KeyValuePair<HashAndIdKey, IPacketBase>(hashAndIdKey, packet));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _logger.Debug(() => $"Stopping {nameof(RealTimeRegistryService)}...");
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~RealTimeRegistryService()
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
