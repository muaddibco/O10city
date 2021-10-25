using System;
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
        public DataAccessService(INodeDataContextRepository dataContextRepository, 
                                 IConfigurationService configurationService,
                                 ILoggerService loggerService)
            : base(dataContextRepository, configurationService, loggerService)
        {
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        public async Task<StealthTransaction> AddTransaction(IKey keyImage, ushort blockType, IKey destinationKey, string content, byte[] hash, CancellationToken cancellationToken = default)
        {
            if (keyImage is null)
            {
                throw new ArgumentNullException(nameof(keyImage));
            }

            string sql =
                "DECLARE @KeyImageId BIGINT\r\n" +
                "DECLARE @IsKeyImageViolated BIT\r\n" +
                "DECLARE @StealthTransactionHashKeyId BIGINT\r\n" +
                "DECLARE @StealthTransactionId BIGINT\r\n" +
                "\r\n" +
                "SELECT @KeyImageId=KeyImageId, @IsKeyImageViolated = CASE WHEN @BlockType IN @BlockTypesAllowed THEN 0 ELSE 1 END FROM KeyImages WHERE Value=@KeyImage\r\n" +
                "IF @IsKeyImageViolated = 1\r\n" +
                "   GOTO EndQuery;\r\n" +
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
                "       VALUES(@KeyImageId, @StealthTransactionHashKeyId, 0, @BlockType, @DestinationKey, @Content)\r\n" +
                "   SET @StealthTransactionId=scope_identity();\r\n" +
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
                    BlockTypesAllowed = new int[] { TransactionTypes.Stealth_KeyImageCompromised, TransactionTypes.Stealth_RevokeIdentity },
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

        public async Task UpdateRegistryInfo(long transactionId, long aggregatedRegistrationHeight, CancellationToken cancellationToken)
        {
            string sql =
                "UPDATE StealthTransactions SET RegistryHeight=@RegistryHeight WHERE StealthTransactionId=@StealthTransactionId;" +
                "UPDATE HK SET HK.RegistryHeight=@RegistryHeight FROM StealthTransactionHashKeys HK\r\n" +
                "INNER JOIN StealthTransactions ST ON ST.HashKeyStealthTransactionHashKeyId=HK.StealthTransactionHashKeyId\r\n" +
                "WHERE ST.StealthTransactionId=@StealthTransactionId";

            await DataContext.ExecuteAsync(sql, new { StealthTransactionId = transactionId, RegistryHeight = aggregatedRegistrationHeight }, cancellationToken: cancellationToken);
        }

        public async Task<StealthTransaction> GetTransaction(long aggregatedRegistryHeight, IKey hash, CancellationToken cancellationToken)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            string sql = 
                "SELECT TOP 1 * FROM StealthTransactions ST\r\n" +
                "INNER JOIN StealthTransactionHashKeys HK ON ST.HashKeyStealthTransactionHashKeyId=HK.StealthTransactionHashKeyId\r\n" +
                "WHERE HK.RegistryHeight=@RegistryHeight AND HK.Hash=@Hash";

            var transaction = await DataContext.QueryFirstOrDefaultAsync<StealthTransaction, StealthTransactionHashKey, StealthTransaction>(
                sql,
                (st, hk) =>
                {
                    st.HashKey = hk;
                    return st;
                },
                "StealthTransactionHashKeyId",
                new { RegistryHeight = aggregatedRegistryHeight, Hash = hash.ToByteArray() },
                cancellationToken: cancellationToken);

            return transaction;
        }

        public async Task<StealthTransaction> GetTransaction(IKey hash, CancellationToken cancellationToken)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            string sql =
                "SELECT TOP 1 * FROM StealthTransactions ST\r\n" +
                "INNER JOIN StealthTransactionHashKeys HK ON ST.HashKeyStealthTransactionHashKeyId=HK.StealthTransactionHashKeyId\r\n" +
                "WHERE HK.Hash=@Hash";

            var transaction = await DataContext.QueryFirstOrDefaultAsync<StealthTransaction, StealthTransactionHashKey, StealthTransaction>(
                sql,
                (st, hk) =>
                {
                    st.HashKey = hk;
                    return st;
                },
                "StealthTransactionHashKeyId",
                new { Hash = hash.ToByteArray() },
                cancellationToken: cancellationToken);

            return transaction;
        }

        public async Task<byte[]> GetHashByKeyImage(byte[] keyImage, CancellationToken cancellationToken)
        {
            string sql =
                "SELECT TOP 1 HK.* FROM StealthTransactionHashKeys HK\r\n" +
                "INNER JOIN StealthTransactions ST ON ST.HashKeyStealthTransactionHashKeyId=HK.StealthTransactionHashKeyId\r\n" +
                "INNER JOIN KeyImages KI ON ST.KeyImageId=KI.KeyImageId\r\n" +
                "WHERE KI.Value=@KeyImage";

            var hash = await DataContext.QueryFirstOrDefaultAsync<StealthTransactionHashKey>(
                sql,
                new { KeyImage = keyImage },
                cancellationToken: cancellationToken);

            return hash?.Hash;
        }
    }
}
