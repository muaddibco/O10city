using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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

        protected override void PostInitTasks()
        {
            LoadAllIdentities();

            base.PostInitTasks();
        }

        #region Account Identities

        public void LoadAllIdentities()
        {
            using var dbContext = GetDataContext();
            _keyIdentityMap = dbContext.AccountIdentities.ToDictionary(i => _identityKeyProvider.GetKey(i.PublicKey.HexStringToByteArray()), i => i);
        }

        public IEnumerable<IKey> GetAllAccountIdentities()
        {
            return _keyIdentityMap.Select(m => m.Key).ToList();
        }

        public AccountIdentity GetAccountIdentity(IKey key)
        {
            if (_keyIdentityMap.ContainsKey(key))
            {
                return _keyIdentityMap[key];
            }

            return null;
        }

        private async Task<AccountIdentity> GetOrAddAccountIdentity(IKey key, CancellationToken cancellationToken = default)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            AccountIdentity identity = GetAccountIdentity(key);

            if (identity == null)
            {
                identity = new AccountIdentity { PublicKey = key.Value.ToArray().ToHexString() };

                using var dbContext = GetDataContext();
                dbContext.AccountIdentities.Add(identity);
                await dbContext.SaveChangesAsync(cancellationToken);
                _keyIdentityMap.Add(key, identity);
            }

            return identity;
        }

        #endregion Account Identities

        public O10TransactionSource GetTransactionSource(IKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            AccountIdentity accountIdentity = GetAccountIdentity(key);

            if (accountIdentity == null)
            {
                return null;
            }

            return GetTransactionalIdentity(accountIdentity);
        }

        private O10TransactionSource GetTransactionalIdentity(AccountIdentity accountIdentity)
        {
            if (accountIdentity == null)
            {
                throw new ArgumentNullException(nameof(accountIdentity));
            }

            using var dbContext = GetDataContext();

            return dbContext.TransactionSources.Include(r => r.Identity).FirstOrDefault(g => g.Identity == accountIdentity);
        }

        private async Task<O10TransactionSource> AddTransactionSource(AccountIdentity accountIdentity, CancellationToken cancellationToken = default)
        {
            O10TransactionSource transactionalIdentity = new O10TransactionSource
            {
                Identity = accountIdentity
            };

            using var dbContext = GetDataContext();
            dbContext.TransactionSources.Add(transactionalIdentity);
            await dbContext.SaveChangesAsync(cancellationToken);

            return transactionalIdentity;
        }

        public void UpdateRegistryInfo(long o10transactionId, long aggregatedRegistrationHeight)
        {
            using var dbContext = GetDataContext();
            var o10transaction = GetLocalAwareO10TransactionPacketById(dbContext, o10transactionId);
            if (o10transaction != null)
            {

                o10transaction.RegistryHeight = aggregatedRegistrationHeight;
                o10transaction.HashKey.RegistryHeight = aggregatedRegistrationHeight;
            }
        }

        public async Task<O10Transaction> AddTransaction(IKey source, ushort packetType, long height, string content, byte[] hash, CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            O10TransactionSource transactionSource = GetTransactionSource(source);

            if (transactionSource == null)
            {
                AccountIdentity accountIdentity = await GetOrAddAccountIdentity(source, cancellationToken);
                transactionSource = await AddTransactionSource(accountIdentity);
            }

            O10TransactionHashKey transactionHashKey = new O10TransactionHashKey
            {
                Hash = hash.ToHexString()
            };

            O10Transaction o10Transaction = new O10Transaction()
            {
                Source = transactionSource,
                HashKey = transactionHashKey,
                Content = content,
                Height = height,
                PacketType = packetType
            };

            using var dbContext = GetDataContext();

            dbContext.BlockHashKeys.Add(transactionHashKey);
            dbContext.TransactionalBlocks.Add(o10Transaction);

            await dbContext.SaveChangesAsync(cancellationToken);

            return o10Transaction;
        }

        public O10Transaction GetLastTransactionalBlock(O10TransactionSource transactionalIdentity)
        {
            if (transactionalIdentity == null)
            {
                throw new ArgumentNullException(nameof(transactionalIdentity));
            }

            using var dbContext = GetDataContext();

            return dbContext.TransactionalBlocks.Where(b => b.Source == transactionalIdentity).OrderByDescending(b => b.Height).FirstOrDefault();
        }

        public O10Transaction GetLastTransactionalBlock(IKey key)
        {
            O10Transaction transactionalBlock = null;

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            O10TransactionSource transactionalIdentity = GetTransactionSource(key);

            if (transactionalIdentity != null)
            {
                transactionalBlock = GetLastTransactionalBlock(transactionalIdentity);
            }

            return transactionalBlock;
        }

        public IEnumerable<O10TransactionSource> GetAllTransctionalIdentities()
        {
            using var dbContext = GetDataContext();

            return dbContext.TransactionSources.ToList();
        }

        public O10Transaction GetTransaction(IKey hash, long registryHeight)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            string hashString = hash.ToString();
            using var dbContext = GetDataContext();
            {
                O10TransactionHashKey hashKey = GetLocalAwareHashKey(dbContext, hashString, registryHeight);
                if (hashKey == null)
                {
                    Logger.Error($"Failed to find Hash Key {hashString} with Registry Height {registryHeight}");
                    return null;
                }

                O10Transaction transactionalBlock = GetLocalAwareTransactionalPacketByHashKey(dbContext, hashKey.O10TransactionHashKeyId);
                return transactionalBlock;
            }
        }

        public O10Transaction GetTransaction(IKey hash)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            string hashString = hash.ToString();
            using var dbContext = GetDataContext();

            O10TransactionHashKey hashKey = GetLocalAwareHashKey(dbContext, hashString);
            if (hashKey == null)
            {
                Logger.Error($"Failed to find Hash Key {hashString}");
                return null;
            }

            O10Transaction transactionalBlock = GetLocalAwareTransactionalPacketByHashKey(dbContext, hashKey.O10TransactionHashKeyId);
            return transactionalBlock;
        }

        #region Private Functions

        private O10Transaction GetLocalAwareTransactionalPacketByHashKey(O10IdDataContextBase dbContext, long hashKeyId)
        {
            Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacketByHashKey)}({hashKeyId})");
            O10Transaction transactionalBlock =
                dbContext.TransactionalBlocks.Local.FirstOrDefault(b => b.HashKey != null && b.HashKey.O10TransactionHashKeyId == hashKeyId);

            if (transactionalBlock == null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacketByHashKey)} not found in local");

                transactionalBlock =
                    dbContext.TransactionalBlocks.FirstOrDefault(b => b.HashKey != null && b.HashKey.O10TransactionHashKeyId == hashKeyId);
            }
            else
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacketByHashKey)} found in local");
            }

            if (transactionalBlock != null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacketByHashKey)}: {transactionalBlock.O10TransactionId}");
            }
            else
            {
                Logger.Warning($"{nameof(GetLocalAwareTransactionalPacketByHashKey)}: TransactionalBlock not found");
            }

            return transactionalBlock;
        }

        private O10Transaction GetLocalAwareO10TransactionPacketById(O10IdDataContextBase dbContext, long o10transactionId)
        {
            Logger.LogIfDebug(() => $"{nameof(GetLocalAwareO10TransactionPacketById)}({o10transactionId})");
            O10Transaction transactionalBlock =
                dbContext.TransactionalBlocks.Local
                    .FirstOrDefault(b => b.O10TransactionId == o10transactionId);

            if (transactionalBlock == null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareO10TransactionPacketById)} not found in local");

                transactionalBlock =
                    dbContext.TransactionalBlocks
                        .FirstOrDefault(b => b.O10TransactionId == o10transactionId);
            }
            else
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareO10TransactionPacketById)} found in local");
            }

            if (transactionalBlock != null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareO10TransactionPacketById)}: {transactionalBlock.O10TransactionId}");
            }
            else
            {
                Logger.Warning($"{nameof(GetLocalAwareO10TransactionPacketById)}: {nameof(O10Transaction)} not found");
            }

            return transactionalBlock;
        }

        private O10TransactionHashKey GetLocalAwareHashKey(O10IdDataContextBase dbContext, string hashString, long registryHeight)
        {
            Logger.LogIfDebug(() => $"GetLocalAwareHashKey({hashString}, {registryHeight})");

            O10TransactionHashKey hashKey 
                = dbContext.BlockHashKeys.Local
                    .FirstOrDefault(h => h.RegistryHeight == registryHeight && h.Hash == hashString);

            if (hashKey == null)
            {
                Logger.LogIfDebug(() => "GetLocalAwareHashKey: not found in local");
                hashKey 
                    = dbContext.BlockHashKeys
                        .FirstOrDefault(h => h.RegistryHeight == registryHeight && h.Hash == hashString);
            }
            else
            {
                Logger.LogIfDebug(() => "GetLocalAwareHashKey: found in local");
            }

            Logger.LogIfDebug(() => $"{nameof(hashKey)}={JsonConvert.SerializeObject(hashKey)}");
            return hashKey;
        }

        private O10TransactionHashKey GetLocalAwareHashKey(O10IdDataContextBase dbContext, string hashString)
        {
            Logger.LogIfDebug(() => $"GetLocalAwareHashKey({hashString})");

            O10TransactionHashKey hashKey
                = dbContext.BlockHashKeys.Local
                    .FirstOrDefault(h => h.Hash == hashString);

            if (hashKey == null)
            {
                Logger.LogIfDebug(() => "GetLocalAwareHashKey: not found in local");
                hashKey
                    = dbContext.BlockHashKeys
                        .FirstOrDefault(h => h.Hash == hashString);
            }
            else
            {
                Logger.LogIfDebug(() => "GetLocalAwareHashKey: found in local");
            }

            Logger.LogIfDebug(() => $"{nameof(hashKey)}={JsonConvert.SerializeObject(hashKey)}");
            return hashKey;
        }

        #endregion Private Function
    }
}
