﻿using System;
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
using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Ledgers.Registry;

namespace O10.Node.Core.Centralized
{
    [RegisterDefaultImplementation(typeof(IRealTimeRegistryService), Lifetime = LifetimeManagement.Singleton)]
    public class RealTimeRegistryService : IRealTimeRegistryService
    {
        private readonly BlockingCollection<Tuple<SynchronizationPacket, RegistryPacket>> _registrationPackets = new BlockingCollection<Tuple<SynchronizationPacket, RegistryPacket>>();
		private readonly ConcurrentDictionary<byte[], TaskCompletionSource<Tuple<byte[], IPacketBase>>> _packetTriggers = new ConcurrentDictionary<byte[], TaskCompletionSource<Tuple<byte[], IPacketBase>>>(new Byte32EqualityComparer());
		private readonly ConcurrentDictionary<long, Dictionary<byte[], IPacketBase>> _packetsPerAggregatedRegistrations = new ConcurrentDictionary<long, Dictionary<byte[], IPacketBase>>();
		private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
		private long _lowestCombinedBlockHeight = long.MaxValue;

		public RealTimeRegistryService(IHashCalculationsRepository hashCalculationsRepository, ILoggerService loggerService)
		{
			_hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _logger = loggerService.GetLogger(nameof(RealTimeRegistryService));
		}

		public long GetLowestCombinedBlockHeight() => _lowestCombinedBlockHeight;

		public IPacketBase GetPacket(long combinedBlockHeight, byte[] hash)
		{
			if(_packetsPerAggregatedRegistrations.TryGetValue(combinedBlockHeight, out Dictionary<byte[], IPacketBase> dict))
			{
				if(dict.TryGetValue(hash, out IPacketBase packet))
				{
					return packet;
				}
			}

			return null;
		}

		public IEnumerable<Tuple<SynchronizationPacket, RegistryPacket>> GetRegistryConsumingEnumerable(CancellationToken cancellationToken)
        {
            return _registrationPackets.GetConsumingEnumerable(cancellationToken);
        }

        public void PostPackets(SynchronizationPacket aggregatedRegistrationsPacket, RegistryPacket registrationsPacket)
        {
			var registryTransaction = registrationsPacket.As<FullRegistryTransaction>();

			_logger.Debug($"Received combined and registryFullBlock with {registryTransaction.Witnesses.Length} Witnesses. Wait for transaction packets...");
			List<Task<Tuple<byte[], IPacketBase>>> tasks = new List<Task<Tuple<byte[], IPacketBase>>>();

			foreach (var item in registryTransaction.Witnesses)
			{
				var transaction = item.Body as RegisterTransaction;
				var hashString = transaction.Parameters[RegisterTransaction.REFERENCED_BODY_HASH];
				var hash = hashString.HexStringToByteArray();
				TaskCompletionSource<Tuple<byte[], IPacketBase>> taskCompletionSource = new TaskCompletionSource<Tuple<byte[], IPacketBase>>();
				_logger.LogIfDebug(() => $"Adding Witness TaskCompletionSource by hash {hashString}");
				taskCompletionSource = _packetTriggers.GetOrAdd(hash, taskCompletionSource);

				tasks.Add(taskCompletionSource.Task);
			}

			ProcessAdding(aggregatedRegistrationsPacket, registrationsPacket, tasks);
        }

		public void PostTransaction(IPacketBase packet)
		{
            if (packet is null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            byte[] hash = _hashCalculation.CalculateHash(packet.ToByteArray());

			_logger.LogIfDebug(() => $"Posted packet {packet.GetType().Name} with hash {hash.ToHexString()}");

			TaskCompletionSource<Tuple<byte[], IPacketBase>> taskCompletionSource = new TaskCompletionSource<Tuple<byte[], IPacketBase>>();
			taskCompletionSource = _packetTriggers.GetOrAdd(hash, taskCompletionSource);
			taskCompletionSource.SetResult(new Tuple<byte[], IPacketBase>(hash, packet));
		}

		private void ProcessAdding(SynchronizationPacket aggregatedRegistrationsPacket, RegistryPacket registrationsPacket, List<Task<Tuple<byte[], IPacketBase>>> tasks)
		{
			Task.WhenAll(tasks).ContinueWith((t, o) => 
			{
				Tuple<SynchronizationPacket, RegistryPacket> tuple = (Tuple<SynchronizationPacket, RegistryPacket>)o;
				var aggregatedRegistrations = tuple.Item1.As<AggregatedRegistrationsTransaction>();
				var registryTransaction = tuple.Item2.As<FullRegistryTransaction>();
				_logger.Debug($"Transaction packet(s) for aggregated registrations of height {aggregatedRegistrations.Height} received");

				Dictionary<byte[], IPacketBase> packets = _packetsPerAggregatedRegistrations.GetOrAdd(aggregatedRegistrations.Height, new Dictionary<byte[], IPacketBase>(new Byte32EqualityComparer()));
				foreach (var item in t.Result)
				{
					byte[] packetHash = item.Item1;
					IPacketBase packet = item.Item2;
					packets.Add(packetHash, packet);
				}

                _logger.Debug($"Passing combined and registryFullBlock with {registryTransaction.Witnesses.Length} Witnesses for further processing...");

                _registrationPackets.Add(tuple);
				if (_lowestCombinedBlockHeight > aggregatedRegistrations.Height)
				{
					_lowestCombinedBlockHeight = aggregatedRegistrations.Height;
				}
			}, new Tuple<SynchronizationPacket, RegistryPacket>(aggregatedRegistrationsPacket, registrationsPacket), TaskScheduler.Current);
		}
	}
}
