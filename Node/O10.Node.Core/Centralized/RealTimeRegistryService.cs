using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using O10.Transactions.Core.DataModel.Registry;
using O10.Transactions.Core.DataModel.Synchronization;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Models;

namespace O10.Node.Core.Centralized
{
    [RegisterDefaultImplementation(typeof(IRealTimeRegistryService), Lifetime = LifetimeManagement.Singleton)]
    public class RealTimeRegistryService : IRealTimeRegistryService
    {
        private readonly BlockingCollection<Tuple<SynchronizationRegistryCombinedBlock, RegistryFullBlock>> _packets = new BlockingCollection<Tuple<SynchronizationRegistryCombinedBlock, RegistryFullBlock>>();
		private readonly ConcurrentDictionary<byte[], TaskCompletionSource<Tuple<byte[], PacketBase>>> _packetTriggers = new ConcurrentDictionary<byte[], TaskCompletionSource<Tuple<byte[], PacketBase>>>(new Byte32EqualityComparer());
		private readonly ConcurrentDictionary<ulong, Dictionary<byte[], PacketBase>> _packetsPerCombinedBlock = new ConcurrentDictionary<ulong, Dictionary<byte[], PacketBase>>();
		private readonly IHashCalculation _hashCalculation;
        private readonly ILogger _logger;
		private ulong _lowestCombinedBlockHeight = ulong.MaxValue;

		public RealTimeRegistryService(IHashCalculationsRepository hashCalculationsRepository, ILoggerService loggerService)
		{
			_hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
            _logger = loggerService.GetLogger(nameof(RealTimeRegistryService));
		}

		public ulong GetLowestCombinedBlockHeight() => _lowestCombinedBlockHeight;

		public PacketBase GetPacket(ulong combinedBlockHeight, byte[] hash)
		{
			if(_packetsPerCombinedBlock.TryGetValue(combinedBlockHeight, out Dictionary<byte[], PacketBase> dict))
			{
				if(dict.TryGetValue(hash, out PacketBase packet))
				{
					return packet;
				}
			}

			return null;
		}

		public IEnumerable<Tuple<SynchronizationRegistryCombinedBlock, RegistryFullBlock>> GetRegistryConsumingEnumerable(CancellationToken cancellationToken)
        {
            return _packets.GetConsumingEnumerable(cancellationToken);
        }

        public void PostPackets(SynchronizationRegistryCombinedBlock combinedBlock, RegistryFullBlock registryFullBlock)
        {
            _logger.Debug($"Received combined and registryFullBlock with {registryFullBlock.StateWitnesses.Count()} StateWitnesses and {registryFullBlock.UtxoWitnesses.Count()} UtxoWitnesses. Wait for transaction packets...");
			List<Task<Tuple<byte[], PacketBase>>> tasks = new List<Task<Tuple<byte[], PacketBase>>>();

			foreach (var item in registryFullBlock.UtxoWitnesses)
			{
				TaskCompletionSource<Tuple<byte[], PacketBase>> taskCompletionSource = new TaskCompletionSource<Tuple<byte[], PacketBase>>();
				_logger.LogIfDebug(() => $"Adding UtxoWitness TaskCompletionSource by hash {item.ReferencedBodyHash.ToHexString()}");
				taskCompletionSource = _packetTriggers.GetOrAdd(item.ReferencedBodyHash, taskCompletionSource);

				tasks.Add(taskCompletionSource.Task);
			}

			foreach (var item in registryFullBlock.StateWitnesses)
			{
				TaskCompletionSource<Tuple<byte[], PacketBase>> taskCompletionSource = new TaskCompletionSource<Tuple<byte[], PacketBase>>();
				_logger.LogIfDebug(() => $"Adding StateWitness TaskCompletionSource by hash {item.ReferencedBodyHash.ToHexString()}");
				taskCompletionSource = _packetTriggers.GetOrAdd(item.ReferencedBodyHash, taskCompletionSource);

				tasks.Add(taskCompletionSource.Task);
			}

			ProcessAdding(combinedBlock, registryFullBlock, tasks);
        }

		public void PostTransaction(PacketBase packet)
		{
			byte[] hash = _hashCalculation.CalculateHash(packet.RawData);

			_logger.LogIfDebug(() => $"Posted packet {packet.GetType().Name} with hash {hash.ToHexString()}");

			TaskCompletionSource<Tuple<byte[], PacketBase>> taskCompletionSource = new TaskCompletionSource<Tuple<byte[], PacketBase>>();
			taskCompletionSource = _packetTriggers.GetOrAdd(hash, taskCompletionSource);
			taskCompletionSource.SetResult(new Tuple<byte[], PacketBase>(hash, packet));
		}

		private void ProcessAdding(SynchronizationRegistryCombinedBlock combinedBlock, RegistryFullBlock registryFullBlock, List<Task<Tuple<byte[], PacketBase>>> tasks)
		{
			Task.WhenAll(tasks).ContinueWith((t, o) => 
			{
				Tuple<SynchronizationRegistryCombinedBlock, RegistryFullBlock> tuple = (Tuple<SynchronizationRegistryCombinedBlock, RegistryFullBlock>)o;
				_logger.Debug($"Transaction packet(s) for combined block height {tuple.Item1.BlockHeight} received");

				Dictionary<byte[], PacketBase> dict = _packetsPerCombinedBlock.GetOrAdd(tuple.Item1.BlockHeight, new Dictionary<byte[], PacketBase>(new Byte32EqualityComparer()));
				foreach (var item in t.Result)
				{
					dict.Add(item.Item1, item.Item2);
				}

                _logger.Debug($"Passing combined and registryFullBlock with {registryFullBlock.StateWitnesses.Count()} StateWitnesses and {registryFullBlock.UtxoWitnesses.Count()} UtxoWitnesses for further processing...");

                _packets.Add(tuple);
				if (_lowestCombinedBlockHeight > combinedBlock.BlockHeight)
				{
					_lowestCombinedBlockHeight = combinedBlock.BlockHeight;
				}
			}, new Tuple<SynchronizationRegistryCombinedBlock, RegistryFullBlock>(combinedBlock, registryFullBlock), TaskScheduler.Current);
		}
	}
}
