using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.Stealth.DataContexts;
using O10.Node.DataLayer.Specific.Stealth.Model;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Tracking;
using O10.Core.DataLayer;
using System.Threading.Tasks;

namespace O10.Node.DataLayer.Specific.Stealth
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessService : NodeDataAccessServiceBase<StealthDataContextBase>
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IHashCalculation _defaultHashCalculation;
        private HashSet<IKey> _keyImages = new HashSet<IKey>(new Key32());
        private readonly Dictionary<StealthTransaction, TaskCompletionSource<StealthTransaction>> _addCompletions = new Dictionary<StealthTransaction, TaskCompletionSource<StealthTransaction>>();

        public DataAccessService(INodeDataContextRepository dataContextRepository,
                                    IConfigurationService configurationService,
                                    ILoggerService loggerService,
                                    ITrackingService trackingService,
                                    IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                    IHashCalculationsRepository hashCalculationsRepository)
            : base(dataContextRepository, configurationService, trackingService, loggerService)
        {
            if (identityKeyProvidersRegistry is null)
            {
                throw new ArgumentNullException(nameof(identityKeyProvidersRegistry));
            }

            if (hashCalculationsRepository is null)
            {
                throw new ArgumentNullException(nameof(hashCalculationsRepository));
            }

            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            _defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        protected override void PostInitTasks()
        {
            LoadAllImageKeys();
            base.PostInitTasks();
        }

        protected override void ProcessEntitySaved(object entity)
        {
            if (entity is StealthTransaction transaction)
            {
                _addCompletions[transaction].SetResult(transaction);
            }
        }

        public void LoadAllImageKeys()
        {
            _keyImages = new HashSet<IKey>(DataContext.StealthKeyImages.Select(k => _identityKeyProvider.GetKey(k.Value.HexStringToByteArray())).AsEnumerable(), new Key32());
        }

        public bool IsStealthImageKeyExist(IKey keyImage)
        {
            return _keyImages.Contains(keyImage);
        }

        public TaskCompletionSource<StealthTransaction> AddStealthBlock(IKey keyImage, ushort blockType, IKey destinationKey, string content, string hashString)
        {
            if (keyImage is null)
            {
                throw new ArgumentNullException(nameof(keyImage));
            }

            //TODO: seems this peace of code is completely unneeded since check for KeyImage duplication is done in another place
            bool isService = blockType == TransactionTypes.Stealth_TransitionCompromisedProofs || blockType == TransactionTypes.Stealth_RevokeIdentity;
            bool keyImageExist = _keyImages.Contains(keyImage);
            if (keyImageExist)
            {
                Logger.Warning($"KeyImage {keyImage} already exist");
                if (isService)
                {
                    Logger.Info($"Service packet {blockType} stored");
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Logger.Info($"KeyImage {keyImage} witnessed");
                _keyImages.Add(keyImage);
            }

            KeyImage StealthKeyImage = new KeyImage { Value = keyImage.Value.ToArray().ToHexString() };

            StealthTransactionHashKey blockHashKey = new StealthTransactionHashKey
            {
                Hash = hashString
            };

            StealthTransaction stealthBlock = new StealthTransaction
            {
                KeyImage = StealthKeyImage,
                HashKey = blockHashKey,
                BlockType = blockType,
                DestinationKey = destinationKey.ToString(),
                Content = content
            };

            var addCompletion = new TaskCompletionSource<StealthTransaction>();
            _addCompletions.Add(stealthBlock, new TaskCompletionSource<StealthTransaction>());

            lock (Sync)
            {
                DataContext.StealthKeyImages.Add(StealthKeyImage);
                DataContext.BlockHashKeys.Add(blockHashKey);
                DataContext.StealthBlocks.Add(stealthBlock);
            }

            return addCompletion;
        }

        public void UpdateRegistryInfo(long transactionId, long aggregatedRegistrationHeight)
        {
            var transaction = GetLocalAwareStealthTransactionPacketById(transactionId);
            if (transaction != null)
            {
                lock (Sync)
                {
                    transaction.RegistryHeight = aggregatedRegistrationHeight;
                    transaction.HashKey.RegistryHeight = aggregatedRegistrationHeight;
                }
            }
        }

        public StealthTransaction GetTransaction(long aggregatedRegistryHeight, IKey hash)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            string hashString = hash.ToString();
            try
            {
                lock (Sync)
                {
                    StealthTransactionHashKey hashKey = GetLocalAwareHashKey(hashString, aggregatedRegistryHeight);
                    if (hashKey == null)
                    {
                        Logger.Error($"Failed to find Hash Key {hashString} with Aggregated Regsitry Height {aggregatedRegistryHeight} or {aggregatedRegistryHeight - 1}");
                        return null;
                    }

                    return GetLocalAwareConfidentialPacket(hashKey.StealthTransactionHashKeyId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failure during obtaining Stealth Packet with hash {hashString} and Aggregated Registry Height {aggregatedRegistryHeight} or {aggregatedRegistryHeight - 1}", ex);
                return null;
            }
        }

        public StealthTransaction GetTransaction(IKey hash)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            string hashString = hash.ToString();
            try
            {
                lock (Sync)
                {
                    StealthTransactionHashKey hashKey = GetLocalAwareHashKey(hashString);
                    if (hashKey == null)
                    {
                        Logger.Error($"Failed to find Hash Key {hashString}");
                        return null;
                    }

                    return GetLocalAwareConfidentialPacket(hashKey.StealthTransactionHashKeyId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failure during obtaining Stealth Packet with hash {hashString}", ex);
                return null;
            }
        }

        private StealthTransaction GetLocalAwareConfidentialPacket(long hashKeyId) =>
            DataContext.StealthBlocks.Local.FirstOrDefault(b => b.HashKey.StealthTransactionHashKeyId == hashKeyId) ??
            DataContext.StealthBlocks.FirstOrDefault(b => b.HashKey.StealthTransactionHashKeyId == hashKeyId);

        public string GetHashByKeyImage(string keyImage)
        {
            lock (Sync)
            {
                return DataContext.StealthBlocks.Include(s => s.KeyImage).Include(s => s.HashKey).FirstOrDefault(p => p.KeyImage.Value == keyImage)?.HashKey.Hash;
            }
        }

        private StealthTransactionHashKey GetLocalAwareHashKey(string hashString, long aggregatedRegistryHeight)
        {
            StealthTransactionHashKey hashKey = DataContext.BlockHashKeys.Local.FirstOrDefault(h =>
                        (h.RegistryHeight == aggregatedRegistryHeight || h.RegistryHeight == (aggregatedRegistryHeight - 1))
                        && h.Hash == hashString);

            if (hashKey == null)
            {
                hashKey = DataContext.BlockHashKeys.FirstOrDefault(h =>
                        (h.RegistryHeight == aggregatedRegistryHeight || h.RegistryHeight == (aggregatedRegistryHeight - 1))
                        && h.Hash == hashString);
            }
            return hashKey;
        }

        private StealthTransactionHashKey GetLocalAwareHashKey(string hashString)
        {
            StealthTransactionHashKey hashKey = DataContext.BlockHashKeys.Local.FirstOrDefault(h => h.Hash == hashString);

            return hashKey;
        }

        private StealthTransaction GetLocalAwareStealthTransactionPacketById(long transactionId)
        {
            Logger.LogIfDebug(() => $"{nameof(GetLocalAwareStealthTransactionPacketById)}({transactionId})");
            var transaction =
                DataContext.StealthBlocks.Local
                    .FirstOrDefault(b => b.StealthTransactionId == transactionId);

            if (transaction == null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareStealthTransactionPacketById)} not found in local");

                transaction =
                    DataContext.StealthBlocks
                        .FirstOrDefault(b => b.StealthTransactionId == transactionId);
            }
            else
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareStealthTransactionPacketById)} found in local");
            }

            if (transaction != null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareStealthTransactionPacketById)}: {transaction.StealthTransactionId}");
            }
            else
            {
                Logger.Warning($"{nameof(GetLocalAwareStealthTransactionPacketById)}: {nameof(StealthTransaction)} not found");
            }

            return transaction;
        }
    }
}
