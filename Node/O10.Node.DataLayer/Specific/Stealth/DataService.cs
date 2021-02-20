using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using O10.Transactions.Core.Ledgers.Stealth;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Node.DataLayer.Specific.Stealth.Model;
using O10.Core.Architecture;
using O10.Core.Models;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Exceptions;

namespace O10.Node.DataLayer.Specific.Stealth
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class DataService : ChainDataServiceBase<DataAccessService>, IStealthDataService
    {
        public DataService(INodeDataAccessServiceRepository dataAccessServiceRepository,
                              Core.Translators.ITranslatorsRepository translatorsRepository,
                              Core.Logging.ILoggerService loggerService)
            : base(dataAccessServiceRepository, translatorsRepository, loggerService)
        {
        }

        public override LedgerType LedgerType => LedgerType.Stealth;

        public override void Add(PacketBase item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Logger.Debug($"Storing {item.GetType().Name}");

            if (item is StealthBase stealth)
            {
                Service.AddStealthBlock(stealth.KeyImage, stealth.SyncHeight, stealth.PacketType, stealth.DestinationKey, stealth.ToString());
            }
        }

        public override IEnumerable<PacketBase> Get(IDataKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key is SyncHashKey syncHashKey)
            {
                StealthTransaction Stealth = Service.GetStealthBySyncAndHash(syncHashKey.SyncBlockHeight, syncHashKey.Hash);

                if (Stealth != null)
                {
                    return new List<PacketBase> { TranslatorsRepository.GetInstance<StealthTransaction, PacketBase>().Translate(Stealth) };
                }
            }
            else if (key is CombinedHashKey combinedHashKey)
            {
                ulong syncBlockHeight = ChainDataServicesManager.GetChainDataService(LedgerType.Synchronization).GetScalar(new SingleByBlockTypeAndHeight(PacketTypes.Synchronization_RegistryCombinationBlock, combinedHashKey.CombinedBlockHeight));
                StealthTransaction Stealth = Service.GetStealthBySyncAndHash(syncBlockHeight, combinedHashKey.Hash);

                if (Stealth == null)
                {
                    Task.Delay(200).Wait();
                    Stealth = Service.GetStealthBySyncAndHash(syncBlockHeight, combinedHashKey.Hash);

                    if (Stealth == null)
                    {
                        Task.Delay(200).Wait();
                        Stealth = Service.GetStealthBySyncAndHash(syncBlockHeight, combinedHashKey.Hash);
                    }
                }

                if (Stealth != null)
                {
                    return new List<PacketBase> { TranslatorsRepository.GetInstance<StealthTransaction, PacketBase>().Translate(Stealth) };
                }
            }

            throw new DataKeyNotSupportedException(key);
        }

        public override void Initialize(CancellationToken cancellationToken)
        {
        }

        public string GetPacketHash(IDataKey dataKey)
        {
            if (dataKey == null)
            {
                throw new ArgumentNullException(nameof(dataKey));
            }

            if (dataKey is KeyImageKey keyImageKey)
            {
                return Service.GetHashByKeyImage(keyImageKey.KeyImage);
            }

            throw new DataKeyNotSupportedException(dataKey);
        }
    }
}
