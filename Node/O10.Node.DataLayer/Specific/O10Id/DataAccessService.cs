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
using O10.Core.Tracking;
using O10.Core.DataLayer;
using System.Threading.Tasks;

namespace O10.Node.DataLayer.Specific.O10Id
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessService : NodeDataAccessServiceBase<O10IdDataContextBase>
    {
        private Dictionary<IKey, AccountIdentity> _keyIdentityMap;
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly Dictionary<O10Transaction, TaskCompletionSource<O10Transaction>> _addCompletions = new Dictionary<O10Transaction, TaskCompletionSource<O10Transaction>>();

        public DataAccessService(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                 INodeDataContextRepository dataContextRepository,
                                 ITrackingService trackingService,
                                 IConfigurationService configurationService,
                                 ILoggerService loggerService)
            : base(dataContextRepository, configurationService, trackingService, loggerService)
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

        protected override void ProcessEntitySaved(object entity)
        {
            if(entity is O10Transaction o10Transaction)
            {
                _addCompletions[o10Transaction].SetResult(o10Transaction);
            }
        }

        #region Account Identities

        public void LoadAllIdentities()
        {
            lock (Sync)
            {
                _keyIdentityMap = DataContext.AccountIdentities.ToDictionary(i => _identityKeyProvider.GetKey(i.PublicKey.HexStringToByteArray()), i => i);
            }
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

        public AccountIdentity GetOrAddAccountIdentity(IKey key)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            AccountIdentity identity = GetAccountIdentity(key);

            if (identity == null)
            {
                identity = new AccountIdentity { PublicKey = key.Value.ToArray().ToHexString() };

                lock (Sync)
                {
                    DataContext.AccountIdentities.Add(identity);
                    _keyIdentityMap.Add(key, identity);
                }
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

        public O10TransactionSource GetTransactionalIdentity(AccountIdentity accountIdentity)
        {
            if (accountIdentity == null)
            {
                throw new ArgumentNullException(nameof(accountIdentity));
            }

            lock (Sync)
            {
                return DataContext.TransactionalIdentities.Include(r => r.Identity).FirstOrDefault(g => g.Identity == accountIdentity);
            }
        }

        public O10TransactionSource AddTransactionSource(AccountIdentity accountIdentity)
        {
            O10TransactionSource transactionalIdentity = new O10TransactionSource
            {
                Identity = accountIdentity
            };

            lock (Sync)
            {
                DataContext.TransactionalIdentities.Add(transactionalIdentity);
            }

            return transactionalIdentity;
        }

        public void UpdateRegistryInfo(long o10transactionId, long aggregatedRegistrationHeight)
        {
            var o10transaction = GetLocalAwareO10TransactionPacketById(o10transactionId);
            if(o10transaction != null)
            {
                lock(Sync)
                {
                    o10transaction.RegistryHeight = aggregatedRegistrationHeight;
                    o10transaction.HashKey.RegistryHeight = aggregatedRegistrationHeight;
                }
            }
        }

        public TaskCompletionSource<O10Transaction> AddTransaction(IKey source, ushort packetType, long height, string content, byte[] hash)
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
                AccountIdentity accountIdentity = GetOrAddAccountIdentity(source);
                transactionSource = AddTransactionSource(accountIdentity);
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

            var addCompletion = new TaskCompletionSource<O10Transaction>();
            _addCompletions.Add(o10Transaction, new TaskCompletionSource<O10Transaction>());

            lock (Sync)
            {
                DataContext.BlockHashKeys.Add(transactionHashKey);
                DataContext.TransactionalBlocks.Add(o10Transaction);
            }

            return addCompletion;
        }

        public O10Transaction GetLastTransactionalBlock(O10TransactionSource transactionalIdentity)
        {
            if (transactionalIdentity == null)
            {
                throw new ArgumentNullException(nameof(transactionalIdentity));
            }

            lock (Sync)
            {
                return DataContext.TransactionalBlocks.Where(b => b.Source == transactionalIdentity).OrderByDescending(b => b.Height).FirstOrDefault();
            }
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
            lock (Sync)
            {
                return DataContext.TransactionalIdentities.ToList();
            }
        }

        public O10Transaction GetTransactionalBySyncAndHash(long registryHeight, IKey hash)
        {
            if (hash is null)
            {
                throw new ArgumentNullException(nameof(hash));
            }

            string hashString = hash.ToString();
            lock (Sync)
            {
                O10TransactionHashKey hashKey = GetLocalAwareHashKey(hashString, registryHeight);
                if (hashKey == null)
                {
                    Logger.Error($"Failed to find Hash Key {hashString} with Registry Height {registryHeight}");
                    return null;
                }

                O10Transaction transactionalBlock = GetLocalAwareTransactionalPacketByHashKey(hashKey.O10TransactionHashKeyId);
                return transactionalBlock;
            }
        }

        #region Private Functions

        private O10Transaction GetLocalAwareTransactionalPacketByHashKey(long hashKeyId)
        {
            Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacketByHashKey)}({hashKeyId})");
            O10Transaction transactionalBlock =
                DataContext.TransactionalBlocks.Local.FirstOrDefault(b => b.HashKey != null && b.HashKey.O10TransactionHashKeyId == hashKeyId);

            if (transactionalBlock == null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacketByHashKey)} not found in local");

                transactionalBlock =
                    DataContext.TransactionalBlocks.FirstOrDefault(b => b.HashKey != null && b.HashKey.O10TransactionHashKeyId == hashKeyId);
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

        private O10Transaction GetLocalAwareO10TransactionPacketById(long o10transactionId)
        {
            Logger.LogIfDebug(() => $"{nameof(GetLocalAwareO10TransactionPacketById)}({o10transactionId})");
            O10Transaction transactionalBlock =
                DataContext.TransactionalBlocks.Local
                    .FirstOrDefault(b => b.O10TransactionId == o10transactionId);

            if (transactionalBlock == null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareO10TransactionPacketById)} not found in local");

                transactionalBlock =
                    DataContext.TransactionalBlocks
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

        private O10TransactionHashKey GetLocalAwareHashKey(string hashString, long registryHeight)
        {
            Logger.LogIfDebug(() => $"GetLocalAwareHashKey({hashString}, {registryHeight})");

            O10TransactionHashKey hashKey 
                = DataContext.BlockHashKeys.Local
                    .FirstOrDefault(h => h.RegistryHeight == registryHeight && h.Hash == hashString);

            if (hashKey == null)
            {
                Logger.LogIfDebug(() => "GetLocalAwareHashKey: not found in local");
                hashKey 
                    = DataContext.BlockHashKeys
                        .FirstOrDefault(h => h.RegistryHeight == registryHeight && h.Hash == hashString);
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
