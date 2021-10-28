using System;
using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.O10Id.DataContexts;
using O10.Node.DataLayer.Specific.O10Id.Model;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Persistency;
using System.Threading.Tasks;
using System.Threading;

namespace O10.Node.DataLayer.Specific.O10Id
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Scoped)]
    public class DataAccessService : NodeDataAccessServiceBase<O10IdDataContextBase>
    {
        private Dictionary<IKey, AccountIdentity> _keyIdentityMap;
        private readonly IIdentityKeyProvider _identityKeyProvider;

        public DataAccessService(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                 INodeDataContextRepository dataContextRepository,
                                 IConfigurationService configurationService,
                                 ILoggerService loggerService)
            : base(dataContextRepository, configurationService, loggerService)
        {
            if (identityKeyProvidersRegistry is null)
            {
                throw new ArgumentNullException(nameof(identityKeyProvidersRegistry));
            }

            _identityKeyProvider = identityKeyProvidersRegistry.GetInstance();
        }

        public override LedgerType LedgerType => LedgerType.O10State;

        #region Account Identities

        #endregion Account Identities

        public async Task UpdateRegistryInfo(long o10transactionId, long aggregatedRegistrationHeight, CancellationToken cancellationToken)
        {
            string sql = 
                "UPDATE O10Transactions SET RegistryHeight=@RegistryHeight WHERE O10TransactionId=@O10TransactionId;\r\n" +
                "UPDATE K SET K.RegistryHeight=@RegistryHeight FROM O10TransactionHashKeys K " +
                "INNER JOIN O10Transactions T ON T.HashKeyO10TransactionHashKeyId=K.O10TransactionHashKeyId " +
                "WHERE T.O10TransactionId=@O10TransactionId;";
            await DataContext.ExecuteAsync(sql, new { O10TransactionId = o10transactionId, RegistryHeight = aggregatedRegistrationHeight }, cancellationToken: cancellationToken);
        }

        public async Task<O10Transaction> AddTransaction(IKey source, ushort packetType, long height, string content, byte[] hash, CancellationToken cancellationToken)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            string sql = 
                "BEGIN TRANSACTION;\r\n" +
                "   DECLARE @O10AccountIdentityID BIGINT\r\n" +
                "   DECLARE @PublicKeyConverted VARBINARY(64) = CONVERT(VARBINARY(64), @PublicKey, 1)\r\n" +
                "   SELECT @O10AccountIdentityID=AccountIdentityId FROM O10AccountIdentity WHERE PublicKey=CONVERT(VARBINARY(64), @PublicKeyConverted, 1)\r\n" +
                "   IF @@rowcount = 0\r\n" +
                "   BEGIN" +
                "       INSERT O10AccountIdentity(KeyHash, PublicKey) VALUES (0, @PublicKeyConverted);\r\n" +
                "       SET @O10AccountIdentityID = scope_identity();\r\n" +
                "   END\r\n" +
                "   \r\n" +
                "   DECLARE @O10TransactionSourceID BIGINT\r\n" +
                "   \r\n" +
                "   SELECT TOP 1" +
                "       @O10TransactionSourceID = O10TransactionSourceId " +
                "   FROM O10TransactionSources TS " +
                "   INNER JOIN O10AccountIdentity AI " +
                "       ON TS.IdentityAccountIdentityId=AI.AccountIdentityId " +
                "   WHERE AI.AccountIdentityId=@O10AccountIdentityID\r\n" +
                "   IF @@rowcount = 0\r\n" +
                "   BEGIN\r\n" +
                "       INSERT O10TransactionSources(IdentityAccountIdentityId) VALUES (@O10AccountIdentityID);\r\n" +
                "       SET @O10TransactionSourceID = scope_identity();\r\n" +
                "   END\r\n" +
                "   \r\n" +
                "   DECLARE @O10TransactionHashKeyID BIGINT\r\n" +
                "   \r\n" +
                "   DECLARE @HashConverted VARBINARY(64) = CONVERT(VARBINARY(64), @Hash, 1)\r\n" +
                "   INSERT O10TransactionHashKeys(RegistryHeight, Hash) VALUES (0, @HashConverted);\r\n" +
                "   SET @O10TransactionHashKeyID = scope_identity();\r\n" +
                "   \r\n" +
                "   DECLARE @O10TransactionID BIGINT\r\n" +
                "   \r\n" +
                "   INSERT INTO O10Transactions(RegistryHeight, SourceO10TransactionSourceId, HashKeyO10TransactionHashKeyId, Content, Height, PacketType) VALUES(0, @O10TransactionSourceID, @O10TransactionHashKeyID, @Content, @Height, @PacketType)\r\n" +
                "   SET @O10TransactionID = scope_identity();\r\n" +
                "COMMIT TRANSACTION;\r\n" +
                "\r\n" +
                "SELECT T.*, HK.*, TS.*, AI.* FROM O10Transactions T\r\n" +
                "INNER JOIN O10TransactionHashKeys HK ON T.HashKeyO10TransactionHashKeyId=HK.O10TransactionHashKeyId\r\n" +
                "INNER JOIN O10TransactionSources TS ON T.SourceO10TransactionSourceId=TS.O10TransactionSourceId\r\n" +
                "INNER JOIN O10AccountIdentity AI ON TS.IdentityAccountIdentityId=AI.AccountIdentityId\r\n" +
                "WHERE T.O10TransactionId=@O10TransactionID";

            var o10Transaction = await DataContext.QueryFirstOrDefaultAsync<O10Transaction, O10TransactionHashKey, O10TransactionSource, AccountIdentity, O10Transaction>(
                sql, 
                (t, hk, ts, ai) => 
                {
                    ts.Identity = ai;
                    t.HashKey = hk;
                    t.Source = ts;
                    return t;
                },
                "O10TransactionHashKeyId,O10TransactionSourceId,AccountIdentityId",
                param: new { PublicKey = $"0x{source}", Content = content, Height = height, PacketType = (int)packetType, Hash = $"0x{hash.ToHexString()}"}, cancellationToken: cancellationToken);

            return o10Transaction;
        }

        public async Task<O10Transaction> GetLastTransactionalBlock(IKey key, CancellationToken cancellationToken)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            string sql = 
                "SELECT TOP 1 T.*, TS.*, AI.* FROM O10Transactions T\r\n" +
                "INNER JOIN O10TransactionSources TS ON T.SourceO10TransactionSourceId=TS.O10TransactionSourceId\r\n" +
                "INNER JOIN O10AccountIdentity AI ON TS.IdentityAccountIdentityId=AI.AccountIdentityId\r\n" +
                "WHERE AI.PublicKey=@PublicKey\r\n" +
                "ORDER BY T.Height DESC";

            var transaction =
                await DataContext.QueryFirstOrDefaultAsync<O10Transaction, O10TransactionSource, AccountIdentity, O10Transaction>(sql,
                    (t, ts, ai) =>
                    {
                        t.Source = ts;
                        ts.Identity = ai;
                        return t;
                    },
                    "O10TransactionSourceId,AccountIdentityId",
                    new { PublicKey = key.ToByteArray() });

            return transaction;
        }

        public async Task<O10Transaction> GetTransaction(IKey hash, long registryHeight, CancellationToken cancellationToken)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            string sql = 
                "SELECT TOP 1 * FROM O10Transactions T\r\n" +
                "INNER JOIN O10TransactionHashKeys HK ON T.HashKeyO10TransactionHashKeyId=HK.O10TransactionHashKeyId AND HK.RegistryHeight=@RegistryHeight\r\n" +
                "WHERE HK.Hash=@Hash";

            var transaction = await DataContext.QueryFirstOrDefaultAsync<O10Transaction>(sql, new { Hash = hash.ToByteArray(), RegistryHeight = registryHeight }, cancellationToken: cancellationToken);

            return transaction;
        }

        public async Task<O10Transaction> GetTransaction(IKey hash, CancellationToken cancellationToken)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            string sql =
                "SELECT TOP 1 * FROM O10Transactions T\r\n" +
                "INNER JOIN O10TransactionHashKeys HK ON T.HashKeyO10TransactionHashKeyId=HK.O10TransactionHashKeyId\r\n" +
                "WHERE HK.Hash=@Hash";

            var transaction = await DataContext.QueryFirstOrDefaultAsync<O10Transaction>(sql, new { Hash = hash.ToByteArray() }, cancellationToken: cancellationToken);

            return transaction;
        }
    }
}
