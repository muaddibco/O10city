using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.O10State;
using O10.Core.Architecture;
using O10.Core.Translators;
using System.Threading;
using O10.Core.Logging;
using System.Threading.Tasks;
using O10.Node.DataLayer.DataServices.Keys;
using Newtonsoft.Json;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Exceptions;
using System;
using O10.Node.DataLayer.Specific.O10Id.Model;
using O10.Core.Serialization;
using O10.Core.HashCalculations;
using O10.Core;
using O10.Transactions.Core.Ledgers;

namespace O10.Node.DataLayer.Specific.O10Id
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class DataService : ChainDataServiceBase<DataAccessService>
    {
        private readonly IHashCalculation _defaultHashCalculation;
        public override LedgerType LedgerType => LedgerType.O10State;

        public DataService(
            INodeDataAccessServiceRepository dataAccessServiceRepository,
            IHashCalculationsRepository hashCalculationsRepository,
            ITranslatorsRepository translatorsRepository,
            ILoggerService loggerService)
            : base(dataAccessServiceRepository, translatorsRepository, loggerService)
        {
            _defaultHashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public override void Add(PacketBase packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet));
            }

            Logger?.LogIfDebug(() => $"Storing {packet.GetType().Name}: {JsonConvert.SerializeObject(packet, new ByteArrayJsonConverter())}");

            if (packet is O10StatePacket transactionalBlockBase)
            {
                var hash = _defaultHashCalculation.CalculateHash(packet.ToString());
                Service.AddTransaction(transactionalBlockBase.Source, (long)transactionalBlockBase.SyncHeight, transactionalBlockBase.PacketType, (long)transactionalBlockBase.Height, packet.ToString(), hash);
                Logger?.LogIfDebug(() => $"Storing of {packet.GetType().Name} completed");
            }
            else
            {
                Logger?.Error("Attempt to store improper packet type");
            }
        }

        public override IEnumerable<PacketBase> Get(IDataKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key is UniqueKey uniqueKey)
            {
                O10Transaction transactionalBlock = Service.GetLastTransactionalBlock(uniqueKey.IdentityKey);

                if (transactionalBlock != null)
                {
                    ITranslator<O10Transaction, PacketBase> mapper = TranslatorsRepository.GetInstance<O10Transaction, PacketBase>();

                    PacketBase block = mapper?.Translate(transactionalBlock);

                    return new List<PacketBase> { block };
                }

                return new List<PacketBase>();
            }
            else if(key is SyncHashKey syncHashKey)
            {
                O10Transaction transactionalBlock = Service.GetTransactionalBySyncAndHash(syncHashKey.SyncBlockHeight, syncHashKey.Hash);

                return new List<PacketBase> { TranslatorsRepository.GetInstance<O10Transaction, PacketBase>().Translate(transactionalBlock) };
            }
            else if (key is CombinedHashKey combinedHashKey)
            {
                ulong syncBlockHeight = ChainDataServicesManager.GetChainDataService(LedgerType.Synchronization).GetScalar(new SingleByBlockTypeAndHeight(TransactionTypes.Synchronization_RegistryCombinationBlock, combinedHashKey.CombinedBlockHeight));
                O10Transaction transactionalBlock = Service.GetTransactionalBySyncAndHash(syncBlockHeight, combinedHashKey.Hash);

				if(transactionalBlock == null)
				{
					Task.Delay(200).Wait();
                    transactionalBlock = Service.GetTransactionalBySyncAndHash(syncBlockHeight, combinedHashKey.Hash);

                    if (transactionalBlock == null)
					{
						Task.Delay(200).Wait();
                        transactionalBlock = Service.GetTransactionalBySyncAndHash(syncBlockHeight, combinedHashKey.Hash);
                    }
				}

                if (transactionalBlock != null)
                {
                    return new List<PacketBase> { TranslatorsRepository.GetInstance<O10Transaction, PacketBase>().Translate(transactionalBlock) };
				}
            }

            throw new DataKeyNotSupportedException(key);
        }

        public override void Initialize(CancellationToken cancellationToken)
        {
        }
    }
}
