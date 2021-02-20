﻿using System;
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

namespace O10.Node.DataLayer.Specific.O10Id
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessService : NodeDataAccessServiceBase<O10IdDataContextBase>
    {
        private Dictionary<IKey, AccountIdentity> _keyIdentityMap;
        private readonly IIdentityKeyProvider _identityKeyProvider;


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

        public void AddTransaction(IKey source, long syncBlockHeight, ushort packetType, long height, string content, byte[] hash)
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

            O10TransactionHashKey blockHashKey = new O10TransactionHashKey
            {
                SyncBlockHeight = (ulong)syncBlockHeight,
                Hash = hash.ToHexString()
            };

            O10Transaction transactionalBlock = new O10Transaction()
            {
                Source = transactionSource,
                HashKey = blockHashKey,
                SyncBlockHeight = syncBlockHeight,
                Content = content,
                Height = height,
                PacketType = packetType
            };

            lock (Sync)
            {
                DataContext.BlockHashKeys.Add(blockHashKey);
                DataContext.TransactionalBlocks.Add(transactionalBlock);
            }
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

        public O10Transaction GetTransactionalBySyncAndHash(ulong syncBlockHeight, byte[] hash)
        {
            string hashString = hash.ToHexString();
            lock (Sync)
            {
                O10TransactionHashKey hashKey = GetLocalAwareHashKey(hashString, syncBlockHeight);
                if (hashKey == null)
                {
                    Logger.Error($"Failed to find Hash Key {hashString} with Sync Block Ids {syncBlockHeight} or {syncBlockHeight - 1}");
                    return null;
                }

                O10Transaction transactionalBlock = GetLocalAwareTransactionalPacket(hashKey.O10TransactionHashKeyId);
                return transactionalBlock;
            }
        }

        #region Private Functions

        private O10Transaction GetLocalAwareTransactionalPacket(long hashKeyId)
        {
            Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacket)}({hashKeyId})");
            O10Transaction transactionalBlock =
                DataContext.TransactionalBlocks.Local.FirstOrDefault(b => b.HashKey != null && b.HashKey.O10TransactionHashKeyId == hashKeyId);

            if (transactionalBlock == null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacket)} not found in local");

                transactionalBlock =
                    DataContext.TransactionalBlocks.FirstOrDefault(b => b.HashKey != null && b.HashKey.O10TransactionHashKeyId == hashKeyId);
            }
            else
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacket)} found in local");
            }

            if (transactionalBlock != null)
            {
                Logger.LogIfDebug(() => $"{nameof(GetLocalAwareTransactionalPacket)}: {transactionalBlock.O10TransactionId}");
            }
            else
            {
                Logger.Warning($"{nameof(GetLocalAwareTransactionalPacket)}: TransactionalBlock not found");
            }

            return transactionalBlock;
        }

        private O10TransactionHashKey GetLocalAwareHashKey(string hashString, ulong syncBlockHeight)
        {
            Logger.LogIfDebug(() => $"GetLocalAwareHashKey({hashString}, {syncBlockHeight})");

            O10TransactionHashKey hashKey = DataContext.BlockHashKeys.Local.FirstOrDefault(h =>
                        (h.SyncBlockHeight == syncBlockHeight || h.SyncBlockHeight == (syncBlockHeight - 1))
                        && h.Hash == hashString);

            if (hashKey == null)
            {
                Logger.LogIfDebug(() => "GetLocalAwareHashKey: not found in local");
                hashKey = DataContext.BlockHashKeys.FirstOrDefault(h =>
                        (h.SyncBlockHeight == syncBlockHeight || h.SyncBlockHeight == (syncBlockHeight - 1))
                        && h.Hash == hashString);
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
