using System;
using System.Collections.Generic;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Core.Architecture;
using O10.Core.Logging;
using System.Collections.ObjectModel;
using O10.Core.HashCalculations;
using O10.Core.ExtensionMethods;
using System.Linq;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Core;

namespace O10.Node.Core.Synchronization
{
    [RegisterDefaultImplementation(typeof(ISyncRegistryMemPool), Lifetime = LifetimeManagement.Singleton)]
    public class SyncRegistryMemPool : ISyncRegistryMemPool
    {
        private readonly int _maxCombinedBlocks = 30;
        private readonly object _syncRound = new object();
        private readonly List<RegistryFullBlock> _registryBlocks = new List<RegistryFullBlock>();
        private readonly List<SynchronizationRegistryCombinedBlock> _registryCombinedBlocks = new List<SynchronizationRegistryCombinedBlock>();
        private readonly IHashCalculation _defaultTransactionHashCalculation;
        private readonly ILogger _logger;

        public SyncRegistryMemPool(ILoggerService loggerService, IHashCalculationsRepository hashCalculationsRepository)
        {
            _logger = loggerService.GetLogger(nameof(SyncRegistryMemPool));
            _defaultTransactionHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public void AddCandidateBlock(RegistryFullBlock registryFullBlock)
        {
            if (registryFullBlock == null)
            {
                throw new ArgumentNullException(nameof(registryFullBlock));
            }

            _logger.Debug($"Adding candidate block of round {registryFullBlock.Height} with {registryFullBlock.StateWitnesses.Length + registryFullBlock.StealthWitnesses.Length} transactions");

            byte[] hash = _defaultTransactionHashCalculation.CalculateHash(registryFullBlock.RawData);

			lock (_registryCombinedBlocks)
			{
				if (_registryCombinedBlocks.Any(b => b.BlockHashes.Any(h => h.Equals32(hash))))
				{
					return;
				}
			}

            lock (_registryBlocks)
            {
                _registryBlocks.Add(registryFullBlock);
            }
        }


        public IEnumerable<RegistryFullBlock> GetRegistryBlocks()
        {
            lock(_registryBlocks)
            {
                List<RegistryFullBlock> blocks = new List<RegistryFullBlock>(_registryBlocks);
                ReadOnlyCollection<RegistryFullBlock> registryFullBlocks = new ReadOnlyCollection<RegistryFullBlock>(blocks);

                _registryBlocks.Clear();

                return registryFullBlocks;
            }
        }

        public void RegisterCombinedBlock(SynchronizationRegistryCombinedBlock combinedBlock)
        {
            List<SynchronizationRegistryCombinedBlock> toRemove = _registryCombinedBlocks.Where(b => (int)(combinedBlock.Height - b.Height) > _maxCombinedBlocks).ToList();

			lock (_registryCombinedBlocks)
			{
				foreach (var item in toRemove)
				{
					_registryCombinedBlocks.Remove(item);
				}

				_registryCombinedBlocks.Add(combinedBlock);
			}
            
            RemoveRange(combinedBlock.BlockHashes);
        }

        #region Private Functions

        private static long NumberOfSetBits(long i)
        {
            i = i - ((i >> 1) & 0x5555555555555555);
            i = (i & 0x3333333333333333) + ((i >> 2) & 0x3333333333333333);
            return (((i + (i >> 4)) & 0xF0F0F0F0F0F0F0F) * 0x101010101010101) >> 56;
        }

        private void RemoveRange(IEnumerable<byte[]> registryFullBlockHashes)
        {
            lock (_registryBlocks)
            {
                List<RegistryFullBlock> toRemove = _registryBlocks.Where(b => registryFullBlockHashes.Any(h => _defaultTransactionHashCalculation.CalculateHash(b.RawData).Equals32(h))).ToList();

                foreach (var item in toRemove)
                {
                    _registryBlocks.Remove(item);
                }
            }
        }

        #endregion Private Functions
    }
}
