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

        public async Task<StealthTransaction> AddStealthBlock(IKey keyImage, ushort blockType, IKey destinationKey, string content, byte[] hash, CancellationToken cancellationToken = default)
        {
            if (keyImage is null)
            {
                throw new ArgumentNullException(nameof(keyImage));
            }

            string sql =
                "DECLARE @KeyImageId BIGINT\r\n" +
                "DECALRE @IsKeyImageViolated BIT\r\n" +
                "DECLARE @StealthTransactionHashKeyId BIGINT\r\n" +
                "DECLARE @StealthTransactionId BIGINT\r\n" +
                "\r\n" +
                "SELECT @KeyImageId=KeyImageId, @IsKeyImageViolated = CASE @BlockType IN (@BlockTypesAllowed) THEN 0 ELSE 1 END FROM KeyImages WHERE Value=@KeyImage\r\n" +
                "IF @IsKeyImageViolated\r\n" +
                "   GOTO EndQuery\r\n" +
                "\r\n" +
                "BEGIN TRANSACTION;\r\n" +
                "   IF @@rowcount = 0\r\n" +
                "   BEGIN\r\n" +
                "       INSERT KeyImages(Value) VALUES(@KeyImage);\r\n" +
                "       SET @KeyImageId=scope_identity();\r\n" +
                "   END\r\n" +
                "   \r\n" +
                "   INSERT StealthTransactionHashKeys(RegistryHeight, Hash) VALUES (0, @Hash);\r\n" +
                "   SET @StealthTransactionHashKeyId=scope_identity();\r\n" +
                "   \r\n" +
                "   INSERT StealthTransactions(KeyImageId, HashKeyStealthTransactionHashKeyId, RegistryHeight, BlockType, DestinationKey, Content)\r\n" +
                "       VALUES(@KeyImageId, @StealthTransactionHashKeyId, 0, @BlockType, @DestinationKey, @Content)" +
                "COMMIT TRANSACTION;\r\n" +
                "\r\n" +
                "EndQuery:\r\n" +
                "SELECT TOP 1 ST.*, KI.KeyImageId AS KIID, KI.Value, HK.* FROM StealthTransactions ST\r\n" +
                "INNER JOIN KeyImages KI ON ST.KeyImageId=KI.KeyImageId\r\n" +
                "INNER JOIN StealthTransactionHashKeys HK ON ST.HashKeyStealthTransactionHashKeyId=HK.StealthTransactionHashKeyId\r\n" +
                "WHERE ST.StealthTransactionId=@StealthTransactionId";

            var stealthBlock = await DataContext.QueryFirstOrDefaultAsync<StealthTransaction, KeyImage, StealthTransactionHashKey, StealthTransaction>(
                sql,
                (t, ki, hk) =>
                {
                    t.KeyImage = ki;
                    t.HashKey = hk;
                    return t;
                },
                "KIID,StealthTransactionHashKeyId",
                new 
                { 
                    BlockTypesAllowed = new[] { TransactionTypes.Stealth_KeyImageCompromised, TransactionTypes.Stealth_RevokeIdentity },
                    KeyImage = keyImage.ToByteArray(),
                    Hash = hash,
                    BlockType = (short)blockType,
                    DestinationKey = destinationKey.ToByteArray(),
                    Content = content
                },
                cancellationToken: cancellationToken);

            if(stealthBlock == null)
            {
                throw new WitnessedKeyImageException(keyImage);
            }

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

        public byte[] GetHashByKeyImage(byte[] keyImage)
        {
            using var dbContext = GetDataContext();
            return dbContext.StealthBlocks.Include(s => s.KeyImage).Include(s => s.HashKey).FirstOrDefault(p => p.KeyImage.Value == keyImage)?.HashKey.Hash;
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
