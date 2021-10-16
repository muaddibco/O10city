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
using O10.Core.Persistency;
using System.Threading.Tasks;
using O10.Transactions.Core.Exceptions;
using System.Threading;

namespace O10.Node.DataLayer.Specific.Stealth
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Scoped)]
    public class DataAccessService : NodeDataAccessServiceBase<StealthDataContextBase>
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IHashCalculation _defaultHashCalculation;
        private HashSet<IKey> _keyImages = new HashSet<IKey>(new Key32());

        public DataAccessService(INodeDataContextRepository dataContextRepository,
                                    IConfigurationService configurationService,
                                    ILoggerService loggerService,
                                    IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                    IHashCalculationsRepository hashCalculationsRepository)
            : base(dataContextRepository, configurationService, loggerService)
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

        protected override async Task PostInitTasks()
        {
            await LoadAllImageKeys();
            await base.PostInitTasks();
        }

        public async Task LoadAllImageKeys()
        {
            string sql = "SELECT Value FROM KeyImages";
            var keyImageValues = await DataContext.QueryAsync<string>(sql, cancellationToken: CancellationToken);

            _keyImages = new HashSet<IKey>(keyImageValues.Select(k => _identityKeyProvider.GetKey(k.HexStringToByteArray())).AsEnumerable(), new Key32());
        }

        public bool IsStealthImageKeyExist(IKey keyImage)
        {
            return _keyImages.Contains(keyImage);
        }

        public async Task<StealthTransaction> AddStealthBlock(IKey keyImage, ushort blockType, IKey destinationKey, string content, string hashString, CancellationToken cancellationToken = default)
        {
            if (keyImage is null)
            {
                throw new ArgumentNullException(nameof(keyImage));
            }

            // TODO: need to make sure that Key Image is checked where it is required to be checked!
            bool isService = blockType == TransactionTypes.Stealth_KeyImageCompromised || blockType == TransactionTypes.Stealth_RevokeIdentity;
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
                    throw new WitnessedKeyImageException(keyImage);
                }
            }
            else
            {
                Logger.Info($"KeyImage {keyImage} witnessed");
                _keyImages.Add(keyImage);
            }

            var keyImageValue = keyImage.ToString();
            KeyImage StealthKeyImage = new KeyImage { Value = keyImageValue, ValueString = keyImageValue };

            StealthTransactionHashKey blockHashKey = new StealthTransactionHashKey
            {
                Hash = hashString,
                HashString = hashString
            };

            StealthTransaction stealthBlock = new StealthTransaction
            {
                KeyImage = StealthKeyImage,
                HashKey = blockHashKey,
                BlockType = blockType,
                DestinationKey = destinationKey.ToString(),
                Content = content
            };

            using var dbContext = GetDataContext();

            dbContext.StealthKeyImages.Add(StealthKeyImage);
            dbContext.StealthTransactionHashKeys.Add(blockHashKey);
            dbContext.StealthBlocks.Add(stealthBlock);
            await dbContext.SaveChangesAsync(cancellationToken);

            return stealthBlock;
        }

        public void UpdateRegistryInfo(long transactionId, long aggregatedRegistrationHeight)
        {
            using var dbContext = GetDataContext();
            var transaction = GetLocalAwareStealthTransactionPacketById(dbContext, transactionId);
            if (transaction != null)
            {
                transaction.RegistryHeight = aggregatedRegistrationHeight;
                transaction.HashKey.RegistryHeight = aggregatedRegistrationHeight;

                dbContext.SaveChanges();
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
                using var dbContext = GetDataContext();

                StealthTransactionHashKey hashKey = GetLocalAwareHashKey(dbContext, hashString, aggregatedRegistryHeight);
                if (hashKey == null)
                {
                    Logger.Error($"Failed to find Hash Key {hashString} with Aggregated Regsitry Height {aggregatedRegistryHeight} or {aggregatedRegistryHeight - 1}");
                    return null;
                }

                return GetLocalAwareConfidentialPacket(dbContext, hashKey.StealthTransactionHashKeyId);
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
                using var dbContext = GetDataContext();

                StealthTransactionHashKey hashKey = GetLocalAwareHashKey(dbContext, hashString);
                if (hashKey == null)
                {
                    Logger.Error($"Failed to find Hash Key {hashString}");
                    return null;
                }

                return GetLocalAwareConfidentialPacket(dbContext, hashKey.StealthTransactionHashKeyId);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failure during obtaining Stealth Packet with hash {hashString}", ex);
                return null;
            }
        }

        private StealthTransaction GetLocalAwareConfidentialPacket(StealthDataContextBase dbContext, long hashKeyId) =>
            dbContext.StealthBlocks.Local.FirstOrDefault(b => b.HashKey.StealthTransactionHashKeyId == hashKeyId) ??
            dbContext.StealthBlocks.FirstOrDefault(b => b.HashKey.StealthTransactionHashKeyId == hashKeyId);

        public string GetHashByKeyImage(string keyImage)
        {
            using var dbContext = GetDataContext();
            return dbContext.StealthBlocks.Include(s => s.KeyImage).Include(s => s.HashKey).FirstOrDefault(p => p.KeyImage.Value == keyImage)?.HashKey.HashString;
        }

        private StealthTransactionHashKey GetLocalAwareHashKey(StealthDataContextBase dbContext, string hashString, long aggregatedRegistryHeight)
        {
            StealthTransactionHashKey hashKey = dbContext.StealthTransactionHashKeys.Local.FirstOrDefault(h =>
                        (h.RegistryHeight == aggregatedRegistryHeight || h.RegistryHeight == (aggregatedRegistryHeight - 1))
                        && h.Hash == hashString);

            if (hashKey == null)
            {
                hashKey = dbContext.StealthTransactionHashKeys.FirstOrDefault(h =>
                        (h.RegistryHeight == aggregatedRegistryHeight || h.RegistryHeight == (aggregatedRegistryHeight - 1))
                        && h.Hash == hashString);
            }
            return hashKey;
        }

        private StealthTransactionHashKey GetLocalAwareHashKey(StealthDataContextBase dbContext, string hashString)
        {
            StealthTransactionHashKey hashKey = dbContext.StealthTransactionHashKeys.Local.FirstOrDefault(h => h.Hash == hashString);

            return hashKey;
        }

        private StealthTransaction GetLocalAwareStealthTransactionPacketById(StealthDataContextBase dbContext, long transactionId)
        {
            Logger.LogIfDebug(() => $"{nameof(GetLocalAwareStealthTransactionPacketById)}({transactionId})");
            var transaction =
                dbContext.StealthBlocks.Local
                    .FirstOrDefault(b => b.StealthTransactionId == transactionId);

            if (transaction == null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareStealthTransactionPacketById)} not found in local");

                transaction =
                    dbContext.StealthBlocks
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
