using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Translators;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Logging;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.Exceptions;
using O10.Transactions.Core.Ledgers;
using O10.Core.Identity;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using AggregatedRegistrationsTransactionDb = O10.Node.DataLayer.Specific.Synchronization.Model.AggregatedRegistrationsTransaction;
using SynchronizationPacketDb = O10.Node.DataLayer.Specific.Synchronization.Model.SynchronizationPacket;
using O10.Transactions.Core.Ledgers.Synchronization;

namespace O10.Node.DataLayer.Specific.Synchronization
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Scoped)]
    public class DataService : ChainDataServiceBase<DataAccessService>
	{
		public DataService(
            INodeDataAccessServiceRepository dataAccessServiceRepository,
            ITranslatorsRepository translatorsRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ILoggerService loggerService)
			: base(dataAccessServiceRepository, translatorsRepository, identityKeyProvidersRegistry, loggerService)
        {
        }

        override public LedgerType LedgerType => LedgerType.Synchronization;

		override public async Task<DataResult<IPacketBase>> Add(IPacketBase item)
        {
            switch (item.Transaction)
            {
                case SynchronizationConfirmedTransaction _:
					await Service.AddSynchronizationBlock(item.AsPacket<SynchronizationPacket>().Payload.Height, DateTime.Now, item.AsPacket<SynchronizationPacket>().Payload.ReportedTime, item.ToJson(), CancellationToken);
					break;
                case AggregatedRegistrationsTransaction transaction:
					await Service.AddSynchronizationRegistryCombinedBlock(item.AsPacket<SynchronizationPacket>().Payload.Height, transaction.SyncHeight, item.ToJson(), transaction.BlockHashes.Select(h => h.ToHexString()).ToArray(), CancellationToken);
					break;
            }

			return new DataResult<IPacketBase>(null, item);
        }

        override public async Task<IEnumerable<IPacketBase>> Get(IDataKey key, CancellationToken cancellationToken)
        {
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

            switch (key)
            {
                case SingleByBlockTypeKey singleByBlockTypeKey:
                    {
                        switch (singleByBlockTypeKey.BlockType)
                        {
                            case TransactionTypes.Synchronization_RegistryCombinationBlock:
                                {
                                    AggregatedRegistrationsTransactionDb block = await Service.GetLastRegistryCombinedBlock(cancellationToken);
                                    return new List<IPacketBase> { TranslatorsRepository.GetInstance<AggregatedRegistrationsTransactionDb, SynchronizationPacket>().Translate(block) };
                                }
                            case TransactionTypes.Synchronization_ConfirmedBlock:
                                {
                                    SynchronizationPacketDb block = await Service.GetLastSynchronizationBlock(cancellationToken);
                                    return new List<IPacketBase> { TranslatorsRepository.GetInstance<SynchronizationPacketDb, SynchronizationPacket>().Translate(block) };
                                }
                            default:
                                return null;
                        }
                    }

                case BlockTypeLowHeightKey blockTypeLowHeightKey:
                    {
                        if (blockTypeLowHeightKey.BlockType == TransactionTypes.Synchronization_ConfirmedBlock)
                        {
                            return (await Service.GetAllLastSynchronizationBlocks(blockTypeLowHeightKey.Height, cancellationToken)).Select(b => TranslatorsRepository.GetInstance<SynchronizationPacketDb, SynchronizationPacket>().Translate(b));
                        }
                        else if (blockTypeLowHeightKey.BlockType == TransactionTypes.Synchronization_RegistryCombinationBlock)
                        {
                            return (await Service.GetAllLastRegistryCombinedBlocks(blockTypeLowHeightKey.Height, cancellationToken)).OrderBy(b => b.AggregatedRegistrationsTransactionId).Select(b => TranslatorsRepository.GetInstance<AggregatedRegistrationsTransactionDb, SynchronizationPacket>().Translate(b));
                        }

                        break;
                    }

                case BlockTypeKey blockTypeKey:
                    {
                        if (blockTypeKey.BlockType == TransactionTypes.Synchronization_ConfirmedBlock)
                        {
                            return Service.GetAllSynchronizationBlocks().Select(b => TranslatorsRepository.GetInstance<SynchronizationPacketDb, SynchronizationPacket>().Translate(b));
                        }
                        else if (blockTypeKey.BlockType == TransactionTypes.Synchronization_RegistryCombinationBlock)
                        {
                            return Service.GetAllRegistryCombinedBlocks().Select(b => TranslatorsRepository.GetInstance<AggregatedRegistrationsTransactionDb, SynchronizationPacket>().Translate(b));
                        }

                        break;
                    }
            }

            throw new DataKeyNotSupportedException(key);
		}

        public override Task AddDataKey(IDataKey key, IDataKey newKey, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
