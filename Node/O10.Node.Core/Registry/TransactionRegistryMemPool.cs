using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.DataModel.Registry;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.States;
using O10.Core.Synchronization;
using O10.Core.HashCalculations;
using O10.Core;
using O10.Transactions.Core.Enums;

namespace O10.Node.Core.Registry
{
    //TODO: add performance counter for measuring MemPool size

    /// <summary>
    /// MemPool is needed for following purposes:
    ///  1. Source for building transactions registry block
    ///  2. Repository for comparing transactions registry key arrived with transactions registry block from another participant
    ///  
    ///  When created Transaction Registry Block gets approved by corresponding node from Sync layer transaction enumerated there must be removed from the Pool
    /// </summary>
    [RegisterDefaultImplementation(typeof(IRegistryMemPool), Lifetime = LifetimeManagement.Singleton)]
    public class TransactionRegistryMemPool : IRegistryMemPool
    {
        private readonly List<RegistryRegisterExBlock> _transactionStateExWitnesses;
        private readonly List<RegistryRegisterBlock> _transactionStateWitnesses;
        private readonly List<RegistryRegisterStealth> _transactionUtxoWitnesses;
        private readonly Dictionary<IKey, List<RegistryRegisterExBlock>> _transactionStateExWitnessesBySender;
        private readonly Dictionary<IKey, List<RegistryRegisterBlock>> _transactionStateWitnessesBySender;
        private readonly Dictionary<IKey, RegistryRegisterStealth> _transactionUtxoWitnessesByKeyImage;

        private readonly Dictionary<ulong, Dictionary<ulong, HashSet<RegistryShortBlock>>> _transactionsShortBlocks;
        private readonly IIdentityKeyProvider _transactionHashKey;
        private readonly ILogger _logger;
        //private readonly Timer _timer;
        private readonly ISynchronizationContext _synchronizationContext;
        private readonly IHashCalculation _hashCalculation;
        private int _oldValue;
        private readonly object _sync = new object();

        public TransactionRegistryMemPool(ILoggerService loggerService, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, 
            IStatesRepository statesRepository, IHashCalculationsRepository hashCalculationsRepository)
        {
            _oldValue = 0;

            _transactionHashKey = identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider();
            _logger = loggerService.GetLogger(nameof(TransactionRegistryMemPool));

            _transactionStateWitnesses = new List<RegistryRegisterBlock>();
            _transactionStateExWitnesses = new List<RegistryRegisterExBlock>();
            _transactionUtxoWitnesses = new List<RegistryRegisterStealth>();
            _transactionStateWitnessesBySender = new Dictionary<IKey, List<RegistryRegisterBlock>>();
            _transactionStateExWitnessesBySender = new Dictionary<IKey, List<RegistryRegisterExBlock>>();
            _transactionUtxoWitnessesByKeyImage = new Dictionary<IKey, RegistryRegisterStealth>();

            _transactionsShortBlocks = new Dictionary<ulong, Dictionary<ulong, HashSet<RegistryShortBlock>>>();
            _synchronizationContext = statesRepository.GetInstance<ISynchronizationContext>();
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public bool EnqueueTransactionWitness(RegistryRegisterBlock transactionWitness)
        {
            lock (_sync)
            {
                bool witnessExist = false;

                if(_transactionStateWitnessesBySender.ContainsKey(transactionWitness.Signer))
                {
                    witnessExist = _transactionStateWitnessesBySender[transactionWitness.Signer].Any(t => t.BlockHeight == transactionWitness.BlockHeight);
                }

                if(!witnessExist)
                {
                    if (!_transactionStateWitnessesBySender.ContainsKey(transactionWitness.Signer))
                    {
                        _transactionStateWitnessesBySender.Add(transactionWitness.Signer, new List<RegistryRegisterBlock>());
                    }

                    _transactionStateWitnessesBySender[transactionWitness.Signer].Add(transactionWitness);
                    _transactionStateWitnesses.Add(transactionWitness);
                    _logger.Debug($"Witness for packet type {transactionWitness.ReferencedPacketType}, block type {transactionWitness.ReferencedBlockType} of sender {transactionWitness.Signer}  and BlockHeight {transactionWitness.BlockHeight} accepted into MemPool");
                }
                else
                {
                    _logger.Warning($"Witness for packet type {transactionWitness.ReferencedPacketType}, block type {transactionWitness.ReferencedBlockType} of sender {transactionWitness.Signer}  and BlockHeight {transactionWitness.BlockHeight} was already witnessed");
                }

                return witnessExist;
            }
        }

        public bool EnqueueTransactionWitness(RegistryRegisterExBlock transactionWitness)
        {
            lock (_sync)
            {
                bool witnessExist = false;

                if (_transactionStateExWitnessesBySender.ContainsKey(transactionWitness.Signer))
                {
                    witnessExist = _transactionStateExWitnessesBySender[transactionWitness.Signer].Any(t => t.BlockHeight == transactionWitness.BlockHeight);
                }

                if (!witnessExist)
                {
                    if (!_transactionStateExWitnessesBySender.ContainsKey(transactionWitness.Signer))
                    {
                        _transactionStateExWitnessesBySender.Add(transactionWitness.Signer, new List<RegistryRegisterExBlock>());
                    }

                    _transactionStateExWitnessesBySender[transactionWitness.Signer].Add(transactionWitness);
                    _transactionStateExWitnesses.Add(transactionWitness);
                    _logger.Debug($"Witness for packet type {transactionWitness.ReferencedPacketType}, action {transactionWitness.ReferencedAction} of sender {transactionWitness.Signer}  and BlockHeight {transactionWitness.BlockHeight} accepted into MemPool");
                }
                else
                {
                    _logger.Warning($"Witness for packet type {transactionWitness.ReferencedPacketType}, action {transactionWitness.ReferencedAction} of sender {transactionWitness.Signer}  and BlockHeight {transactionWitness.BlockHeight} was already witnessed");
                }

                return witnessExist;
            }
        }

        public bool EnqueueTransactionWitness(RegistryRegisterStealth transactionWitness)
        {
            lock (_sync)
            {
				bool isService = transactionWitness.BlockType == ActionTypes.Stealth_TransitionCompromisedProofs || transactionWitness.BlockType == ActionTypes.Stealth_RevokeIdentity;
				if(isService)
				{
					_logger.Info($"Service packet {transactionWitness.BlockType} witnessed");
				}

                bool keyImageExist = _transactionUtxoWitnessesByKeyImage.ContainsKey(transactionWitness.KeyImage);
                if (isService || !keyImageExist)
                {
                    if(!keyImageExist)
                    {
                        _logger.Info($"Witness for packet type {transactionWitness.ReferencedPacketType}, block type {transactionWitness.ReferencedBlockType} and KeyImage {transactionWitness.KeyImage} accepted into MemPool");

                        _transactionUtxoWitnessesByKeyImage.Add(transactionWitness.KeyImage, transactionWitness);
                    }

                    _transactionUtxoWitnesses.Add(transactionWitness);

                    return true;
                }
                else
                {
                    _logger.Warning($"Witness with Key Image {transactionWitness.KeyImage} was already witnessed");
                }
            }

            return false;
        }

        public bool EnqueueTransactionsShortBlock(RegistryShortBlock transactionsShortBlock)
        {
            lock (_sync)
            {
                if (!_transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight].ContainsKey(transactionsShortBlock.BlockHeight))
                {
                    _transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight].Add(transactionsShortBlock.BlockHeight, new HashSet<RegistryShortBlock>());
                }

                return _transactionsShortBlocks[transactionsShortBlock.SyncBlockHeight][transactionsShortBlock.BlockHeight].Add(transactionsShortBlock);
            }
        }

        public void ClearWitnessed(RegistryShortBlock transactionsShortBlock)
        {
            lock (_sync)
            {
                foreach (var witnessStateKey in transactionsShortBlock.WitnessStateKeys)
                {
                    if(_transactionStateWitnessesBySender.ContainsKey(witnessStateKey.PublicKey))
                    {
                        RegistryRegisterBlock transactionWitness = _transactionStateWitnessesBySender[witnessStateKey.PublicKey].FirstOrDefault(t => t.BlockHeight == witnessStateKey.Height);

                        if(transactionWitness != null)
                        {
                            _transactionStateWitnessesBySender[witnessStateKey.PublicKey].Remove(transactionWitness);
                            if(_transactionStateWitnessesBySender[witnessStateKey.PublicKey].Count == 0)
                            {
                                _transactionStateWitnessesBySender.Remove(witnessStateKey.PublicKey);
                            }

                            _transactionStateWitnesses.Remove(transactionWitness);
                        }
                    }
                }

                foreach (var witnessUtxoKey in transactionsShortBlock.WitnessUtxoKeys)
                {
                    if (_transactionUtxoWitnessesByKeyImage.ContainsKey(witnessUtxoKey.KeyImage))
                    {
                        RegistryRegisterStealth transactionWitness = _transactionUtxoWitnessesByKeyImage[witnessUtxoKey.KeyImage];

                        _transactionStateWitnessesBySender.Remove(witnessUtxoKey.KeyImage);
                        _transactionUtxoWitnesses.Remove(transactionWitness);
                    }
                }
            }
        }

        //TODO: need to understand whether it is needed to pass height of Sync Block or automatically take latest one?
        public SortedList<ushort, RegistryRegisterBlock> DequeueStateWitnessBulk()
        {
            SortedList<ushort, RegistryRegisterBlock> items = new SortedList<ushort, RegistryRegisterBlock>();
            lock (_sync)
            {
                ushort order = 0;

                foreach (var transactionWitness in _transactionStateWitnesses)
                {
                    items.Add(order++, transactionWitness);

                    if (order == ushort.MaxValue)
                    {
                        break;
                    }
                }
            }

            _logger.Debug($"MemPool returns {items.Count} State Witness items");
            return items;
        }
        public SortedList<ushort, RegistryRegisterStealth> DequeueUtxoWitnessBulk()
        {
            SortedList<ushort, RegistryRegisterStealth> items = new SortedList<ushort, RegistryRegisterStealth>();
            lock (_sync)
            {
                ushort order = 0;

                foreach (var transactionWitness in _transactionUtxoWitnesses)
                {
                    items.Add(order++, transactionWitness);

                    if (order == ushort.MaxValue)
                    {
                        break;
                    }
                }
            }

            _logger.Debug($"MemPool returns {items.Count} UTXO Witness items");
            return items;
        }

        public RegistryShortBlock GetRegistryShortBlockByHash(ulong syncBlockHeight, ulong round, byte[] hash)
        {
            if (!_transactionsShortBlocks.ContainsKey(syncBlockHeight))
            {
                return null;
            }

            if(!_transactionsShortBlocks[syncBlockHeight].ContainsKey(round))
            {
                return null;
            }

            RegistryShortBlock registryShortBlock = _transactionsShortBlocks[syncBlockHeight][round].FirstOrDefault(s => _hashCalculation.CalculateHash(s.RawData).Equals32(hash));

            return registryShortBlock;
        }

		public bool IsKeyImageWitnessed(IKey keyImage)
		{
			return _transactionUtxoWitnessesByKeyImage?.ContainsKey(keyImage) ?? false;
		}
	}
}
