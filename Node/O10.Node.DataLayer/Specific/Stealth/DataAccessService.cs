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

namespace O10.Node.DataLayer.Specific.Stealth
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessService : NodeDataAccessServiceBase<StealthDataContextBase>
    {
        private readonly IIdentityKeyProvider _identityKeyProvider;
        private readonly IHashCalculation _defaultHashCalculation;
        private HashSet<IKey> _keyImages = new HashSet<IKey>(new Key32());

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

        public override LedgerType PacketType => LedgerType.Stealth;

        protected override void PostInitTasks()
        {
            LoadAllImageKeys();
            base.PostInitTasks();
        }

        public void LoadAllImageKeys()
        {
            _keyImages = new HashSet<IKey>(DataContext.StealthKeyImages.Select(k => _identityKeyProvider.GetKey(k.Value.HexStringToByteArray())).AsEnumerable(), new Key32());
        }

        public bool IsStealthImageKeyExist(IKey keyImage)
        {
            return _keyImages.Contains(keyImage);
        }

        public bool AddStealthBlock(IKey keyImage, ulong syncBlockHeight, ushort blockType, byte[] destinationKey, byte[] blockContent)
        {
            if (keyImage is null)
            {
                throw new ArgumentNullException(nameof(keyImage));
            }

            bool isService = blockType == PacketTypes.Stealth_TransitionCompromisedProofs || blockType == PacketTypes.Stealth_RevokeIdentity;
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
                    return false;
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
                SyncBlockHeight = syncBlockHeight,
                Hash = _defaultHashCalculation.CalculateHash(blockContent).ToHexString()
            };

            StealthTransaction StealthBlock = new StealthTransaction
            {
                KeyImage = StealthKeyImage,
                HashKey = blockHashKey,
                SyncBlockHeight = syncBlockHeight,
                BlockType = blockType,
                DestinationKey = destinationKey.ToHexString(),
                Content = blockContent
            };

            lock (Sync)
            {
                DataContext.StealthKeyImages.Add(StealthKeyImage);
                DataContext.BlockHashKeys.Add(blockHashKey);
                DataContext.StealthBlocks.Add(StealthBlock);
            }

            return true;
        }

        public StealthTransaction GetStealthBySyncAndHash(ulong syncBlockHeight, byte[] hash)
        {
            string hashString = hash.ToHexString();
            try
            {
                lock (Sync)
                {
                    StealthTransactionHashKey hashKey = GetLocalAwareHashKey(hashString, syncBlockHeight);
                    if (hashKey == null)
                    {
                        Logger.Error($"Failed to find Hash Key {hashString} with Sync Block Ids {syncBlockHeight} or {syncBlockHeight - 1}");
                        return null;
                    }

                    return GetLocalAwareConfidentialPacket(hashKey.StealthTransactionHashKeyId);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Failure during obtaining Confidential Packet with hash {hashString} and Sync Block Height {syncBlockHeight} or {syncBlockHeight - 1}", ex);
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

        private StealthTransactionHashKey GetLocalAwareHashKey(string hashString, ulong syncBlockHeight)
        {
            StealthTransactionHashKey hashKey = DataContext.BlockHashKeys.Local.FirstOrDefault(h =>
                        (h.SyncBlockHeight == syncBlockHeight || h.SyncBlockHeight == (syncBlockHeight - 1))
                        && h.Hash == hashString);

            if (hashKey == null)
            {
                hashKey = DataContext.BlockHashKeys.FirstOrDefault(h =>
                        (h.SyncBlockHeight == syncBlockHeight || h.SyncBlockHeight == (syncBlockHeight - 1))
                        && h.Hash == hashString);
            }
            return hashKey;
        }
    }
}
