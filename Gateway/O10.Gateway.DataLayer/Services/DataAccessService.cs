using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using O10.Gateway.DataLayer.Configuration;
using O10.Gateway.DataLayer.Model;
using O10.Gateway.DataLayer.Services.Inputs;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Logging;
using O10.Transactions.Core.Enums;
using O10.Core.Identity;

namespace O10.Gateway.DataLayer.Services
{
    [RegisterDefaultImplementation(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessService : IDataAccessService, IDisposable
    {
        private readonly int _lockTimeout = 120000;
		private bool _isInitialzed;
        private readonly object _sync = new object();
        private readonly object _syncSaving = new object();
        private DataContext _dataContext;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IHashCalculation _hashCalculation;
        private readonly List<long> _utxoOutputsIndiciesMap;
		private readonly IEnumerable<IDataContext> _dataContexts;
		private readonly IGatewayDataContextConfiguration _configuration;
        private ILogger _logger;
        private bool _isSaving;
		private Dictionary<string, List<IKey>> _rootAttributes = new Dictionary<string, List<IKey>>();
		private Dictionary<WitnessPacket, TaskCompletionSource<WitnessPacket>> _witnessPacketStoreCompletions = new Dictionary<WitnessPacket, TaskCompletionSource<WitnessPacket>>();

        public DataAccessService(IEnumerable<IDataContext> dataContexts, IConfigurationService configurationService, IHashCalculationsRepository hashCalculationsRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ILoggerService loggerService)
        {
			_isInitialzed = false;
			_utxoOutputsIndiciesMap = new List<long>();
            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
			_hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
			_dataContexts = dataContexts;
			_configuration = configurationService.Get<IGatewayDataContextConfiguration>();
            _logger = loggerService.GetLogger(nameof(DataAccessService));
            _logger.Debug("ctor DataAccessService");
		}

		public void Initialize(CancellationToken cancellationToken)
        {
            if (_isInitialzed)
            {
                _logger.Debug("DataAccessService already initialized");
                return;
            }

            lock (_sync)
            {
                if (_isInitialzed)
                {
                    _logger.Debug("DataAccessService already initialized");
                    return;
                }

                _logger.Debug("DataAccessService initialization started");

                _dataContext = _dataContexts.FirstOrDefault(d => d.DataProvider.Equals(_configuration.ConnectionType)) as DataContext;
                _dataContext.Initialize(_configuration.ConnectionString);
                _dataContext.Database.Migrate();
                _logger.Info($"ConnectionString = {_dataContext.Database.GetDbConnection().ConnectionString}");

                InitializeUtxoOutputsIndiciesMap();

                InitializeRootAttributes();

                InitializeLocalCache();

                _isInitialzed = true;
            }

            InitializeTrackingAndSave(cancellationToken);
        }

        private void InitializeLocalCache()
        {
            _dataContext.PacketHashes.Load();
            _dataContext.WitnessPackets.Load();
        }

        private void InitializeTrackingAndSave(CancellationToken cancellationToken)
        {
            _dataContext.ChangeTracker.StateChanged += ChangeTracker_StateChanged;

            PeriodicTaskFactory.Start(() =>
            {
                if (_isSaving)
                {
                    return;
                }

                lock (_syncSaving)
                {
                    if (_isSaving)
                        return;

					_isSaving = true;
				}

				ProcessSaving();
			}, 1000, cancelToken: cancellationToken);
        }

        private void ProcessSaving()
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    _dataContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.Error("Failure during saving data to database", ex);
                }
                finally
                {
                    _isSaving = false;
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to save DataContext changes due to lock timeout");
            }
        }

        private void InitializeUtxoOutputsIndiciesMap()
        {
            foreach (var utxoOutput in _dataContext.UtxoOutputs.Where(o => !o.IsOverriden))
            {
                _utxoOutputsIndiciesMap.Add(utxoOutput.StealthOutputId);
            }
        }

        private void InitializeRootAttributes()
        {
            foreach (var rootAttribute in _dataContext.RootAttributeIssuances.Include(r => r.Issuer).Where(r => !r.IsOverriden))
            {
                if (!_rootAttributes.ContainsKey(rootAttribute.Issuer.Key))
                {
                    _rootAttributes.Add(rootAttribute.Issuer.Key, new List<IKey>());
                }

                _rootAttributes[rootAttribute.Issuer.Key].Add(_identityKeyProvider.GetKey(rootAttribute.RootCommitment.HexStringToByteArray()));
            }
        }

        private void ChangeTracker_StateChanged(object sender, Microsoft.EntityFrameworkCore.ChangeTracking.EntityStateChangedEventArgs e)
		{
            //_logger.LogIfDebug(() => $"State of {e.Entry.Entity.GetType().Name} changed {e.OldState} -> {e.NewState}");

            if (e.OldState == EntityState.Added && e.NewState == EntityState.Unchanged)
			{
				if(e.Entry.Entity is WitnessPacket witnessPacket)
				{
					if(_witnessPacketStoreCompletions.ContainsKey(witnessPacket))
					{
						_witnessPacketStoreCompletions[witnessPacket].SetResult(witnessPacket);
					}
				}
				else if(e.Entry.Entity is StealthOutput utxoOutput)
				{
					_utxoOutputsIndiciesMap.Add(utxoOutput.StealthOutputId);
				}
			}
		}

		public bool GetLastRegistryCombinedBlock(out long height, out string content)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    RegistryCombinedBlock combinedBlock = _dataContext.RegistryCombinedBlocks.OrderByDescending(b => b.RegistryCombinedBlockId).FirstOrDefault();

                    if (combinedBlock != null)
                    {
                        height = combinedBlock.RegistryCombinedBlockId;
                        content = combinedBlock.Content;
                        return true;
                    }
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetLastRegistryCombinedBlock");
            }

            height = 0;
            content = null;
            return false;
        }

        public bool GetLastSyncBlock(out ulong height, out byte[] hash)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    SyncBlock syncBlock = _dataContext.SyncBlocks.OrderByDescending(b => b.SyncBlockId).FirstOrDefault();

                    if (syncBlock != null)
                    {
                        height = (ulong)syncBlock.SyncBlockId;
                        hash = syncBlock.Hash.HexStringToByteArray();
                        return true;
                    }

                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetLastSyncBlock");
            }

            height = 0;
            hash = null;
            return false;
        }

        public void StoreIncomingTransactionalBlock(StateIncomingStoreInput storeInput)
        {
            if (storeInput is null)
            {
                throw new ArgumentNullException(nameof(storeInput));
            }

            StatePacket transactionalIncomingBlock = new StatePacket
            {
                WitnessId = storeInput.WitnessId,
                Height = storeInput.BlockHeight,
                BlockType = storeInput.BlockType,
                Content = storeInput.Content,
                Source = GetOrAddAddress(storeInput.Source) ?? throw new ArgumentException($"{nameof(storeInput.Source)} is missing"),
                Target = GetOrAddAddress(storeInput.Destination),
                TransactionKey = GetOrAddUtxoTransactionKey(storeInput.TransactionKey),
                Output = GetOrAddUtxoOutput(storeInput.Commitment, storeInput.Destination, storeInput.OriginatingCommitment),
                ThisBlockHash = GetOrAddPacketHash(
                                    _identityKeyProvider.GetKey(_hashCalculation.CalculateHash(storeInput.Content)),
                                    storeInput.CombinedRegistryBlockHeight),
                IsVerified = true,
                IsValid = true,
                IsTransition = storeInput.TransactionKey != null
            };

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    _dataContext.TransactionalPackets.Add(transactionalIncomingBlock);
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at StoreIncomingTransactionalBlock");
            }
        }

        public void StoreIncomingUtxoTransactionBlock(UtxoIncomingStoreInput storeInput)
        {
            if (storeInput is null)
            {
                throw new ArgumentNullException(nameof(storeInput));
            }

            StealthPacket utxoIncomingBlock = new StealthPacket
            {
                WitnessId = storeInput.WitnessId,
                BlockType = storeInput.BlockType,
                TransactionKey = GetOrAddUtxoTransactionKey(storeInput.TransactionKey) ?? throw new ArgumentException($"{nameof(storeInput.TransactionKey)} is missing"),
                KeyImage = GetOrAddUtxoKeyImage(storeInput.KeyImage),
                Output = GetOrAddUtxoOutput(storeInput.Commitment, storeInput.Destination) ?? throw new ArgumentException($"{nameof(storeInput.Commitment)} and {nameof(storeInput.Destination)} are missing"),
                Content = storeInput.Content,
                ThisBlockHash = GetOrAddPacketHash(
                    _identityKeyProvider.GetKey(_hashCalculation.CalculateHash(storeInput.Content)),
                    storeInput.CombinedRegistryBlockHeight)
            };

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    _dataContext.StealthPackets.Add(utxoIncomingBlock);
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at StoreIncomingUtxoTransactionBlock");
            }
        }

        public void StoreRegistryCombinedBlock(long height, string content)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    if(_dataContext.RegistryCombinedBlocks.Local.Any(r => r.RegistryCombinedBlockId == height) ||
                        _dataContext.RegistryCombinedBlocks.Any(r => r.RegistryCombinedBlockId == height))
                    {
                        _logger.Warning($"RegistryCombinedBlock with height {height} already exist");

                        return;
                    }

                    _dataContext.RegistryCombinedBlocks.Add(
                        new RegistryCombinedBlock { RegistryCombinedBlockId = height, Content = content });
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at StoreRegistryCombinedBlock");
            }
        }

        public void StoreRegistryFullBlock(ulong height, byte[] content)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    _dataContext.RegistryFullBlocks.Add(new RegistryFullBlockData { CombinedBlockHeight = (long)height, Content = content });
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at StoreRegistryFullBlock");
            }
        }

        public TaskCompletionSource<WitnessPacket> StoreWitnessPacket(long syncBlockHeight, long round, long combinedBlockHeight, LedgerType referencedLedgerType, ushort referencedPacketType, IKey referencedBodyHash, IKey referencedDestinationKey, IKey referencedDestinationKey2, IKey referencedTransactionKey, IKey referencedKeyImage)
        {
            if (referencedBodyHash is null)
            {
                throw new ArgumentNullException(nameof(referencedBodyHash));
            }

            try
            {
                WitnessPacket witnessPacket = new WitnessPacket
                {
                    SyncBlockHeight = syncBlockHeight,
                    Round = round,
                    CombinedBlockHeight = combinedBlockHeight,
                    ReferencedLedgerType = referencedLedgerType,
                    ReferencedPacketType = referencedPacketType,
                    ReferencedBodyHash = GetOrAddPacketHash(referencedBodyHash, combinedBlockHeight),
                    ReferencedDestinationKey = referencedDestinationKey?.ToString(),
                    ReferencedDestinationKey2 = referencedDestinationKey2?.ToString(),
                    ReferencedTransactionKey = referencedTransactionKey?.ToString(),
                    ReferencedKeyImage = referencedKeyImage?.ToString()
				};

                TaskCompletionSource<WitnessPacket> taskCompletionSource = new TaskCompletionSource<WitnessPacket>();


                if (Monitor.TryEnter(_sync, _lockTimeout))
                {
                    try
                    {
                        _dataContext.WitnessPackets.Add(witnessPacket);
                        _witnessPacketStoreCompletions.Add(witnessPacket, taskCompletionSource);
                    }
                    finally
                    {
                        Monitor.Exit(_sync);
                    }
                }
                else
                {
                    _logger.Warning("Failed to acquire lock at StoreWitnessPacket");
                }

                return taskCompletionSource;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failure at {nameof(StoreWitnessPacket)}", ex);
                throw;
            }
		}

		public Dictionary<long, List<WitnessPacket>> GetWitnessPackets(long combinedBlockHeightStart, long combinedBlockHeightEnd)
		{
			if (Monitor.TryEnter(_sync, _lockTimeout))
			{
				try
				{
					return _dataContext.WitnessPackets.Local.Where(w => w.CombinedBlockHeight >= combinedBlockHeightStart && w.CombinedBlockHeight <= combinedBlockHeightEnd).GroupBy(w => w.CombinedBlockHeight).ToDictionary(g => g.Key, g => g.ToList());
				}
				finally
				{
					Monitor.Exit(_sync);
				}
			}
			else
			{
				_logger.Warning("Failed to acquire lock at StoreWitnessPacket");
			}

			return null;
		}

		public WitnessPacket GetWitnessPacket(long witnessPacketId)
		{
			if (Monitor.TryEnter(_sync, _lockTimeout))
			{
				try
				{
					return _dataContext.WitnessPackets.Local.FirstOrDefault(w => w.WitnessPacketId == witnessPacketId);
				}
				finally
				{
					Monitor.Exit(_sync);
				}
			}
			else
			{
				_logger.Warning("Failed to acquire lock at GetWitnessPacket");
			}

			return null;
		}

		public void UpdateLastSyncBlock(long height, IKey hash)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            if (Monitor.TryEnter(_sync, _lockTimeout))
			{
				try
				{
					if (!_dataContext.SyncBlocks.Local.Any(s => s.SyncBlockId == height) && !_dataContext.SyncBlocks.Any(s => s.SyncBlockId == height))
					{
						_dataContext.SyncBlocks.Add(new SyncBlock { SyncBlockId = height, Hash = hash.ToString() });
					}

                    List<SyncBlock> supersedeSyncBlocks = _dataContext.SyncBlocks.Where(s => s.SyncBlockId > (long)height).ToList();
                    if(supersedeSyncBlocks.Count > 0)
                    {
                        _dataContext.SyncBlocks.RemoveRange(supersedeSyncBlocks);
                    }
                }
				finally
				{
					Monitor.Exit(_sync);
				}
			}
			else
			{
				_logger.Warning("Failed to acquire lock at UpdateLastSyncBlock");
			}
        }

        public void CutExcessedPackets(long combinedBlockHeight)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    List<StatePacket> excessedTransactionalPackets = _dataContext.TransactionalPackets.Where(r => r.ThisBlockHash.CombinedRegistryBlockHeight > combinedBlockHeight).ToList();
                    _dataContext.TransactionalPackets.RemoveRange(excessedTransactionalPackets);

                    List<StealthPacket> excessedStealthPackets = _dataContext.StealthPackets.Where(r => r.ThisBlockHash.CombinedRegistryBlockHeight > combinedBlockHeight).ToList();
                    _dataContext.StealthPackets.RemoveRange(excessedStealthPackets);

                    List<PacketHash> excessedHashes = _dataContext.PacketHashes.Where(r => r.CombinedRegistryBlockHeight > combinedBlockHeight).ToList();
                    _dataContext.PacketHashes.RemoveRange(excessedHashes);

                    List<WitnessPacket> excessedWitnessPackets = _dataContext.WitnessPackets.Local.Where(r => r.CombinedBlockHeight > combinedBlockHeight).ToList();
                    _dataContext.WitnessPackets.RemoveRange(excessedWitnessPackets);

                    List<RegistryCombinedBlock> excessedCombinedBlocks = _dataContext.RegistryCombinedBlocks.Where(r => r.RegistryCombinedBlockId > combinedBlockHeight).ToList();
                    _dataContext.RegistryCombinedBlocks.RemoveRange(excessedCombinedBlocks);
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at CutSupersedeBlocks");
            }
        }

        public int GetTotalUtxoOutputsAmount()
        {
            return _utxoOutputsIndiciesMap.Count;
        }

		public byte[][] GetRootAttributeCommitments(byte[] issuer, int amount)
		{
            string issuerStr = issuer.ToHexString();
			int max = Math.Min(_rootAttributes[issuerStr].Count, amount);

			byte[][] commitments = new byte[max][];
			Random random = new Random(issuer.GetHashCode() ^ _rootAttributes[issuerStr].Count);
			List<int> pickedIndicies = new List<int>();


			for (int i = 0; i < max; i++)
			{
				bool found = false;

				do
				{
					int index = random.Next(_rootAttributes[issuerStr].Count);
					if(pickedIndicies.Contains(index))
					{
						continue;
					}

					pickedIndicies.Add(index);
					found = true;
					commitments[i] = _rootAttributes[issuerStr][index].ToByteArray();
				} while (!found);
			}

			return commitments;
		}

		public StealthOutput[] GetOutputs(int amount)
		{
			int max = amount;

            if(_utxoOutputsIndiciesMap.Count < amount)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), $"Number of requested outputs ({amount}) cannot exceed number of avaialble ones ({_utxoOutputsIndiciesMap.Count})");
            }

			StealthOutput[] outputs = new StealthOutput[amount];
			Random random = new Random(_utxoOutputsIndiciesMap.Count);
			List<int> pickedIndicies = new List<int>();

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    for (int i = 0; i < max; i++)
                    {
                        bool found = false;

                        do
                        {
                            int index = random.Next(_utxoOutputsIndiciesMap.Count);
                            if (pickedIndicies.Contains(index))
                            {
                                continue;
                            }

							StealthOutput utxoOutput = _dataContext.UtxoOutputs.FirstOrDefault(o => o.StealthOutputId == _utxoOutputsIndiciesMap[index]);
							pickedIndicies.Add(index);

							if (outputs.All(o => o?.Commitment != utxoOutput.Commitment))
							{
								found = true;
								outputs[i] = _dataContext.UtxoOutputs.FirstOrDefault(o => o.StealthOutputId == _utxoOutputsIndiciesMap[index]);
							}
                        } while (!found);
                    }
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetOutputs");
            }

            return outputs;
		}

        /// <summary>
        /// Returns content of RegistryFulBlocks and height of corresponding CombinedBlocks
        /// </summary>
        /// <param name="heightStart"></param>
        /// <param name="heights"></param>
        /// <param name="contents"></param>
        /// <returns></returns>
        public bool GetRegistryFullBlocks(ulong heightStart, out ulong[] heights, out byte[][][] contents)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    IEnumerable<RegistryCombinedBlock> registryCombinedBlocks = _dataContext.RegistryCombinedBlocks.Where(r => r.RegistryCombinedBlockId >= (long)heightStart);

                    if (registryCombinedBlocks == null || !registryCombinedBlocks.Any())
                    {
                        heights = null;
                        contents = null;
                        return false;
                    }

                    contents = GetRegistryBlocksContent(registryCombinedBlocks);

                    heights = registryCombinedBlocks.Select(c => (ulong)c.RegistryCombinedBlockId).ToArray();

                    return true;

                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetRegistryFullBlocks");
            }

            heights = null;
            contents = null;
            return false;
        }

        private byte[][][] GetRegistryBlocksContent(IEnumerable<RegistryCombinedBlock> registryCombinedBlocks)
        {
            byte[][][] contents = new byte[registryCombinedBlocks.Count()][][];
            int i = 0;
            foreach (RegistryCombinedBlock registryCombinedBlock in registryCombinedBlocks)
            {
                IEnumerable<RegistryFullBlockData> registryFullBlocks = _dataContext.RegistryFullBlocks.Where(r => r.CombinedBlockHeight == registryCombinedBlock.RegistryCombinedBlockId);

                if (registryFullBlocks != null)
                {
                    contents[i] = new byte[registryFullBlocks.Count()][];

                    int j = 0;
                    foreach (RegistryFullBlockData registryFullBlock in registryFullBlocks)
                    {
                        contents[i][j++] = registryFullBlock.Content;
                    }

                    i++;
                }
            }

            return contents;
        }

		internal void WipeAll()
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    _dataContext.Database.EnsureDeleted();
                    _dataContext.Database.EnsureCreated();
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at WipeAll");
            }
        }

        public void StoreRootAttributeIssuance(IKey issuer, IKey issuanceCommitment, IKey rootCommitment, long combinedBlockHeight)
        {
            if (issuanceCommitment is null)
            {
                throw new ArgumentNullException(nameof(issuanceCommitment));
            }

            if (rootCommitment is null)
            {
                throw new ArgumentNullException(nameof(rootCommitment));
            }

            string? addr = issuer?.ToString();
            string issuanceCommitmentStr = issuanceCommitment.ToString();
            string rootCommitmentStr = rootCommitment.ToString();

            _logger.LogIfDebug(() => $"{nameof(StoreRootAttributeIssuance)} for issuer={addr?.ToVarBinary()}, issuanceCommitment={issuanceCommitmentStr.ToVarBinary()}, rootCommitment={rootCommitmentStr.ToVarBinary()}, and combinedBlockHeight={combinedBlockHeight}");

            Address address = GetOrAddAddress(issuer) ?? throw new ArgumentNullException(nameof(issuer));

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    RootAttribute rootAttributeIssuance = new RootAttribute
                    {
                        IssuanceCommitment = issuanceCommitmentStr,
                        RootCommitment = rootCommitmentStr,
                        IssuanceCombinedBlock = combinedBlockHeight,
                        Issuer = address
                    };

                    _dataContext.RootAttributeIssuances.Add(rootAttributeIssuance);

                    if (!_rootAttributes.ContainsKey(addr))
                    {
                        _rootAttributes.Add(addr, new List<IKey>());
                    }

                    _rootAttributes[addr].Add(rootCommitment);
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at StoreRootAttributeIssuance");
            }
        }

        public void SetRootAttributesOverriden(IKey issuer, IKey issuanceCommitment, long combinedBlockHeight)
        {
            if (issuer is null)
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            if (issuanceCommitment is null)
            {
                throw new ArgumentNullException(nameof(issuanceCommitment));
            }

            string issuanceCommitmentString = issuanceCommitment.ToString();
			string addr = issuer.ToString();
            
            _logger.LogIfDebug(() => $"{nameof(SetRootAttributesOverriden)} for issuer={addr.ToVarBinary()}, issuanceCommitment={issuanceCommitmentString.ToVarBinary()}, and combinedBlockHeight={combinedBlockHeight}");

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    List<RootAttribute> overriding = _dataContext.RootAttributeIssuances.Include(r => r.Issuer).Where(r => r.Issuer.Key == addr && r.IssuanceCommitment == issuanceCommitmentString && r.RevocationCombinedBlock == 0).ToList();
                    overriding.ForEach(r => { r.IsOverriden = true; r.RevocationCombinedBlock = combinedBlockHeight; });
                    List<byte[]> overriden = overriding.Select(r => r.RootCommitment.HexStringToByteArray()).ToList();

                    if (overriden.Any() && _rootAttributes.ContainsKey(addr))
                    {
                        _rootAttributes[addr].RemoveAll(c => overriden.Any(o => c.Equals(o)));

						overriden.ForEach(overridenEntry => 
						{
							string overridenEntryString = overridenEntry.ToHexString();
							IEnumerable<StealthOutput> outputs = _dataContext.UtxoOutputs.Where(c => c.Commitment == overridenEntryString);
							foreach (var o in outputs)
							{
								o.IsOverriden = true;
								_utxoOutputsIndiciesMap.Remove(o.StealthOutputId);
							}
						});
                    }
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at SetRootAttributesOverriden");
            }
        }

        public void StoreAssociatedAttributeIssuance(IKey issuer, IKey issuanceCommitment, IKey rootIssuanceCommitment)
        {
            if (issuer is null)
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            if (issuanceCommitment is null)
            {
                throw new ArgumentNullException(nameof(issuanceCommitment));
            }

            if (rootIssuanceCommitment is null)
            {
                throw new ArgumentNullException(nameof(rootIssuanceCommitment));
            }

            string issuanceCommitmentString = issuanceCommitment.ToString();
			string rootIssuanceCommitmentString = rootIssuanceCommitment.ToString();
			string? addr = issuer?.ToString();

            _logger.LogIfDebug(() => $"{nameof(StoreAssociatedAttributeIssuance)} for issuer={addr?.ToVarBinary()}, issuanceCommitment={issuanceCommitmentString.ToVarBinary()} and rootIssuanceCommitment={rootIssuanceCommitmentString.ToVarBinary()}");
            _logger.LogIfDebug(() => $"{nameof(StoreAssociatedAttributeIssuance)} for issuer={addr}, issuanceCommitment={issuanceCommitmentString} and rootIssuanceCommitment={rootIssuanceCommitmentString}");

            Address address = GetOrAddAddress(issuer) ?? throw new ArgumentNullException(nameof(issuer));

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    AssociatedAttributeIssuance associatedAttributeIssuance = new AssociatedAttributeIssuance
                    {
                        IssuanceCommitment = issuanceCommitmentString,
                        RootIssuanceCommitment = rootIssuanceCommitmentString,
                        Issuer = address
                    };

                    _dataContext.AssociatedAttributeIssuances.Add(associatedAttributeIssuance);
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at StoreAssociatedAttributeIssuance");
            }
        }

        public bool CheckRootAttributeExist(byte[] issuer, byte[] rootCommitment)
        {
			string rootCommitmentString = rootCommitment.ToHexString();
            _logger.LogIfDebug(() => $"{nameof(CheckRootAttributeExist)} for issuer={issuer.ToHexString().ToVarBinary()} and rootCommitment={rootCommitmentString.ToVarBinary()}");

            if (issuer == null)
            {
                if (Monitor.TryEnter(_sync, _lockTimeout))
                {
                    try
                    {
                        return _dataContext.RootAttributeIssuances.Any(a => !a.IsOverriden && a.RootCommitment == rootCommitmentString);
                    }
                    finally
                    {
                        Monitor.Exit(_sync);
                    }
                }
                else
                {
                    _logger.Warning("Failed to acquire lock at CheckRootAttributeExist");
                }
            }
            else
            {
                string addr = issuer.ToHexString();
                return _rootAttributes.ContainsKey(addr) && _rootAttributes[addr].Any(r => r.Equals(rootCommitment));
            }

            return false;
        }

        public bool CheckRootAttributeWasValid(byte[] issuer, byte[] rootCommitment, long combinedBlockHeight)
        {
            string issuerString = issuer.ToHexString();
            string rootCommitmentString = rootCommitment.ToHexString();
            _logger.LogIfDebug(() => $"{nameof(CheckRootAttributeWasValid)} for {nameof(combinedBlockHeight)}={combinedBlockHeight}, issuer={issuerString.ToVarBinary()}, and rootCommitment={rootCommitmentString.ToVarBinary()}");

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    return _dataContext.RootAttributeIssuances.Include(a => a.Issuer).Any(a => a.Issuer.Key == issuerString &&
                        a.RootCommitment == rootCommitmentString && a.IssuanceCombinedBlock <= combinedBlockHeight &&
                        (a.RevocationCombinedBlock == 0 || a.RevocationCombinedBlock > combinedBlockHeight));
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at CheckRootAttributeWasValid");
            }

            return false;
        }

        public bool CheckAssociatedAtributeExist(Memory<byte>? issuer, Memory<byte> issuanceCommitment, Memory<byte> rootCommitment)
        {
			string issuanceCommitmentString = issuanceCommitment.ToHexString();
			string rootIssuanceCommitmentString = rootCommitment.ToHexString();
			string addr = issuer?.ToHexString();

            _logger.LogIfDebug(() => $"{nameof(CheckAssociatedAtributeExist)} for issuer={addr}, {nameof(issuanceCommitment)}={issuanceCommitmentString.ToVarBinary()}, and rootIssuanceCommitment={rootIssuanceCommitmentString.ToVarBinary()}");

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    if (issuer == null)
                    {
                        return _dataContext.AssociatedAttributeIssuances.Any(a => a.IssuanceCommitment == issuanceCommitmentString && a.RootIssuanceCommitment == rootIssuanceCommitmentString);
                    }
                    else
                    {
                        return _dataContext.AssociatedAttributeIssuances.Include(a => a.Issuer).AsEnumerable().Any(a => a.Issuer.Key == addr && a.IssuanceCommitment == issuanceCommitmentString && a.RootIssuanceCommitment == rootIssuanceCommitmentString);
                    }

                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at CheckAssociatedAtributeExist");
            }

            return false;
        }

        public StatePacket GetTransactionalIncomingBlock(long witnessid)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
					StatePacket transactionalIncomingBlock = _dataContext.TransactionalPackets.Local.FirstOrDefault(t => t.WitnessId == witnessid);

					if(transactionalIncomingBlock == null)
					{
						transactionalIncomingBlock = _dataContext.TransactionalPackets.FirstOrDefault(t => t.WitnessId == witnessid);
					}

					return transactionalIncomingBlock;
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetTransactionalIncomingBlock");
            }

            return null;
        }

        public StealthPacket GetUtxoIncomingBlock(long witnessid)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
					StealthPacket utxoIncomingBlock = _dataContext.StealthPackets.Local.FirstOrDefault(t => t.WitnessId == witnessid);

					if(utxoIncomingBlock == null)
					{
						utxoIncomingBlock = _dataContext.StealthPackets.FirstOrDefault(t => t.WitnessId == witnessid);
					}

					return utxoIncomingBlock;
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetUtxoIncomingBlock");
            }

            return null;
        }

        public StealthPacket GetStealthPacket(long combinedRegistryBlockHeight, string hashString)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    var packetHash = _dataContext.PacketHashes.Local.FirstOrDefault(h => h.CombinedRegistryBlockHeight == combinedRegistryBlockHeight && h.Hash == hashString);

                    if(packetHash != null)
                    {
                        StealthPacket stealthPacket = _dataContext.StealthPackets.Local.FirstOrDefault(t => t.ThisBlockHash?.PacketHashId == packetHash.PacketHashId);

                        if (stealthPacket == null)
                        {
                            stealthPacket = _dataContext.StealthPackets.Include(p => p.ThisBlockHash).FirstOrDefault(t => t.ThisBlockHash != null && t.ThisBlockHash.PacketHashId == packetHash.PacketHashId);
                        }

                        return stealthPacket;
                    }
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning($"Failed to acquire lock at {nameof(GetStealthPacket)}");
            }

            return null;
        }

        public void CancelEmployeeRecord(IKey issuerKey, IKey registrationCommitment)
        {
            if (issuerKey is null)
            {
                throw new ArgumentNullException(nameof(issuerKey));
            }

            if (registrationCommitment is null)
            {
                throw new ArgumentNullException(nameof(registrationCommitment));
            }

            var issuer = issuerKey.ToString();
            string registrationCommitmentStr = registrationCommitment.ToString();

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    RelationRecord employeeRecord = _dataContext.EmployeeRecords.Local.FirstOrDefault(e => e.Issuer == issuer && e.RegistrationCommitment == registrationCommitmentStr);

                    if(employeeRecord == null)
                    {
                        employeeRecord = _dataContext.EmployeeRecords.FirstOrDefault(e => e.Issuer == issuer && e.RegistrationCommitment == registrationCommitmentStr);
                    }

                    if(employeeRecord != null)
                    {
                        employeeRecord.IsRevoked = true;
                    }
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at CancelEmployeeRecord");
            }
        }

        public void AddEmployeeRecord(IKey issuer, IKey registrationCommitment, IKey groupCommitment)
		{
            if (issuer is null)
            {
                throw new ArgumentNullException(nameof(issuer));
            }

            if (registrationCommitment is null)
            {
                throw new ArgumentNullException(nameof(registrationCommitment));
            }

            if (groupCommitment is null)
            {
                throw new ArgumentNullException(nameof(groupCommitment));
            }

            if (Monitor.TryEnter(_sync, _lockTimeout))
			{
				try
				{
					RelationRecord employeeRecord = new RelationRecord
					{
						Issuer = issuer.ToString(),
						RegistrationCommitment = registrationCommitment.ToString(),
						GroupCommitment = groupCommitment.ToString()
					};

					_dataContext.EmployeeRecords.Add(employeeRecord);
				}
				finally
				{
					Monitor.Exit(_sync);
				}
			}
			else
			{
				_logger.Warning("Failed to acquire lock at AddEmployeeRecord");
			}
		}

		public byte[] GetEmployeeRecordGroup(string issuer, string registrationCommitment)
		{
			if (Monitor.TryEnter(_sync, _lockTimeout))
			{
				try
				{
					RelationRecord employeeRecord = _dataContext.EmployeeRecords.Local.FirstOrDefault(e => !e.IsRevoked && e.Issuer == issuer && e.RegistrationCommitment == registrationCommitment);

					if(employeeRecord == null)
					{
						employeeRecord = _dataContext.EmployeeRecords.FirstOrDefault(e => !e.IsRevoked && e.Issuer == issuer && e.RegistrationCommitment == registrationCommitment);
					}

					return employeeRecord?.GroupCommitment.HexStringToByteArray();
				}
				finally
				{
					Monitor.Exit(_sync);
				}
			}
			else
			{
				_logger.Warning("Failed to acquire lock at GetEmployeeRecordGroup");
			}

			return null;
		}

		public StatePacket GetTransactionBySourceAndHeight(string source, ulong blockHeight)
		{
			if (Monitor.TryEnter(_sync, _lockTimeout))
			{
				try
				{
					StatePacket transaction = _dataContext.TransactionalPackets.Include(t => t.Source).Include(t => t.ThisBlockHash).FirstOrDefault(t => source.Equals(t.Source.Key) && t.Height == (long)blockHeight);

					return transaction;
				}
				finally
				{
					Monitor.Exit(_sync);
				}
			}
			else
			{
				_logger.Warning("Failed to acquire lock at GetTransactionBySourceAndHeight");
			}

			return null;
		}

        public void AddCompromisedKeyImage(IKey keyImage)
        {
            if (keyImage is null)
            {
                throw new ArgumentNullException(nameof(keyImage));
            }

            var keyImageStr = keyImage.ToString();

            lock(_sync)
            {
                if(!_dataContext.CompromisedKeyImages.Any(i => i.KeyImage == keyImageStr))
                {
                    _dataContext.CompromisedKeyImages.Add(new CompromisedKeyImage { KeyImage = keyImageStr });

                    _dataContext.SaveChanges();
                }
            }
        }

        public bool GetIsKeyImageCompomised(IKey keyImage)
        {
            if (keyImage is null)
            {
                throw new ArgumentNullException(nameof(keyImage));
            }

            var keyImageStr = keyImage.ToString();

            lock (_sync)
            {
                return _dataContext.CompromisedKeyImages.Any(c => c.KeyImage == keyImageStr);
            }
        }

        #region Private Functions

        private Address? GetOrAddAddress(IKey? addrKey)
        {
            if(addrKey == null)
            {
                return null;
            }

            var addr = addrKey.ToString();
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    Address identity = _dataContext.Identities.FirstOrDefault(i => i.Key == addr);

                    if (identity == null)
                    {
                        identity = new Address
                        {
                            Key = addr
                        };

                        _dataContext.Identities.Add(identity);
                        _dataContext.SaveChanges();
                    }

                    return identity;

                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetOrAddAddress");
            }

            return null;
        }

        private PacketHash GetOrAddPacketHash(IKey blockHash, long combinedRegistryBlockHeight)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    string blockHashString = blockHash.ToString();
                    PacketHash block = _dataContext.PacketHashes.FirstOrDefault(b => b.CombinedRegistryBlockHeight == b.CombinedRegistryBlockHeight && b.Hash == blockHashString);

                    if (block == null)
                    {
                        block = new PacketHash
                        {
                            CombinedRegistryBlockHeight = combinedRegistryBlockHeight,
                            Hash = blockHashString
                        };

                        _dataContext.PacketHashes.Add(block);
                    }

                    return block;

                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetOrAddBlockHash");
            }

            return null;
        }

        private KeyImage GetOrAddUtxoKeyImage(IKey keyImage)
        {
            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    string keyImageString = keyImage.ToString();
                    KeyImage utxoKeyImage = _dataContext.UtxoKeyImages.FirstOrDefault(b => b.Value == keyImageString);

                    if (utxoKeyImage == null)
                    {
                        utxoKeyImage = new KeyImage
                        {
                            Value = keyImageString
                        };

                        _dataContext.UtxoKeyImages.Add(utxoKeyImage);
                    }

                    return utxoKeyImage;
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetOrAddUtxoKeyImage");
            }

            return null;
        }

        private TransactionKey? GetOrAddUtxoTransactionKey(IKey? transactionKey)
        {
            if(transactionKey == null)
            {
                return null;
            }

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                try
                {
                    string transactionKeyString = transactionKey.ToString();
                    TransactionKey utxoTransactionKey = _dataContext.UtxoTransactionKeys.FirstOrDefault(b => b.Key == transactionKeyString);

                    if (utxoTransactionKey == null)
                    {
                        utxoTransactionKey = new TransactionKey
                        {
                            Key = transactionKeyString
                        };

                        _dataContext.UtxoTransactionKeys.Add(utxoTransactionKey);
                    }

                    return utxoTransactionKey;
                }
                finally
                {
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetOrAddUtxoTransactionKey");
            }

            return null;
        }

        private StealthOutput? GetOrAddUtxoOutput(IKey? commitment, IKey? destinationKey, IKey? originatingCommitment = null)
        {
            if(commitment == null && destinationKey == null)
            {
                return null;
            }

            string commitmentString = commitment?.ToString()??throw new ArgumentNullException(nameof(commitment));
            string destinationKeyString = destinationKey?.ToString() ?? throw new ArgumentNullException(nameof(destinationKey));

            if (Monitor.TryEnter(_sync, _lockTimeout))
            {
                _logger.Debug("Entering GetOrAddUtxoOutput");

                try
                {
                    StealthOutput utxoOutput = _dataContext.UtxoOutputs.FirstOrDefault(b => !b.IsOverriden && b.Commitment == commitmentString && b.DestinationKey == destinationKeyString);

                    if (utxoOutput == null)
                    {
                        utxoOutput = new StealthOutput
                        {
                            Commitment = commitmentString,
                            DestinationKey = destinationKeyString,
                            OriginatingCommitment = originatingCommitment?.ToString()
                        };

                        _dataContext.UtxoOutputs.Add(utxoOutput);
                    }

                    return utxoOutput;
                }
                finally
                {
                    _logger.Debug("Exiting GetOrAddUtxoOutput");
                    Monitor.Exit(_sync);
                }
            }
            else
            {
                _logger.Warning("Failed to acquire lock at GetOrAddUtxoOutput");
            }

            return null;
        }

        #endregion Private Functions

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					_dataContext?.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				_disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~DataAccessService() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}

		#endregion
	}
}
