using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Translators;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Logging;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.Exceptions;
using O10.Node.DataLayer.Specific.Synchronization.Model;
using O10.Transactions.Core.Ledgers;

namespace O10.Node.DataLayer.Specific.Synchronization
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class DataService : ChainDataServiceBase<DataAccessService>
	{
		private readonly BlockingCollection<PacketBase> _packets = new BlockingCollection<PacketBase>();
		private BufferBlock<SynchronizationConfirmedBlock> _bufferSyncPackets;
		private BufferBlock<SynchronizationRegistryCombinedBlock> _bufferCombinedPackets;

		public DataService(
            INodeDataAccessServiceRepository dataAccessServiceRepository,
            ITranslatorsRepository translatorsRepository,
            ILoggerService loggerService)
			: base(dataAccessServiceRepository, translatorsRepository, loggerService)
        {
        }

        override public LedgerType LedgerType => LedgerType.Synchronization;

		override public void Add(PacketBase item)
        {
			_packets.Add(item);
        }

        public override ulong GetScalar(IDataKey key)
        {
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

            if (key is SingleByBlockTypeAndHeight blockTypeAndHeight && blockTypeAndHeight.BlockType == TransactionTypes.Synchronization_RegistryCombinationBlock)
            {
                return Service.GetRegistryCombinedBlockByHeight(blockTypeAndHeight.Height)?.SyncBlockHeight ?? 0L;
            }

            return base.GetScalar(key);
		}

        override public IEnumerable<PacketBase> Get(IDataKey key)
        {
			if (key == null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			if (key is SingleByBlockTypeKey singleByBlockTypeKey)
            {
				switch (singleByBlockTypeKey.BlockType)
				{
					case TransactionTypes.Synchronization_RegistryCombinationBlock:
						{
							RegistryCombinedBlock block = Service.GetLastRegistryCombinedBlock();
							return new List<PacketBase> { TranslatorsRepository.GetInstance<RegistryCombinedBlock, PacketBase>().Translate(block) };
						}
					case TransactionTypes.Synchronization_ConfirmedBlock:
						{
							SynchronizationBlock block = Service.GetLastSynchronizationBlock();
							return new List<PacketBase> { TranslatorsRepository.GetInstance<SynchronizationBlock, PacketBase>().Translate(block) };
						}
					default:
						return null;
				}
			}
			else if (key is BlockTypeLowHeightKey blockTypeLowHeightKey)
			{
				if (blockTypeLowHeightKey.BlockType == TransactionTypes.Synchronization_ConfirmedBlock)
				{
					return Service.GetAllLastSynchronizationBlocks(blockTypeLowHeightKey.Height).Select(b => TranslatorsRepository.GetInstance<SynchronizationBlock, PacketBase>().Translate(b));
				}
				else if (blockTypeLowHeightKey.BlockType == TransactionTypes.Synchronization_RegistryCombinationBlock)
				{
					return Service.GetAllLastRegistryCombinedBlocks(blockTypeLowHeightKey.Height).OrderBy(b => b.RegistryCombinedBlockId).Select(b => TranslatorsRepository.GetInstance<RegistryCombinedBlock, PacketBase>().Translate(b));
				}
			}
			else if (key is BlockTypeKey blockTypeKey)
			{
				if (blockTypeKey.BlockType == TransactionTypes.Synchronization_ConfirmedBlock)
				{
					return Service.GetAllSynchronizationBlocks().Select(b => TranslatorsRepository.GetInstance<SynchronizationBlock, PacketBase>().Translate(b));
				}
				else if (blockTypeKey.BlockType == TransactionTypes.Synchronization_RegistryCombinationBlock)
				{
					return Service.GetAllRegistryCombinedBlocks().Select(b => TranslatorsRepository.GetInstance<RegistryCombinedBlock, PacketBase>().Translate(b));
				}
			}

			throw new DataKeyNotSupportedException(key);
		}

		override public void Initialize(CancellationToken cancellationToken)
        {
			_bufferSyncPackets = new BufferBlock<SynchronizationConfirmedBlock>(new DataflowBlockOptions { CancellationToken = cancellationToken });
			_bufferCombinedPackets = new BufferBlock<SynchronizationRegistryCombinedBlock>(new DataflowBlockOptions { CancellationToken = cancellationToken });

			ConsumeSynchronizationConfirmedBlocks(_bufferSyncPackets, cancellationToken);
			ConsumeSynchronizationRegistryCombinedBlock(_bufferCombinedPackets, cancellationToken);

			Task.Factory.StartNew(() =>
			{
				try
				{
					foreach (var item in _packets.GetConsumingEnumerable(cancellationToken))
					{
						if (item is SynchronizationConfirmedBlock synchronizationConfirmedBlock)
						{
							Logger.LogIfDebug(() => $"Adding to buffer synchronization block {synchronizationConfirmedBlock.Height}");
							_bufferSyncPackets.Post(synchronizationConfirmedBlock);
							//Service.AddSynchronizationBlock(synchronizationConfirmedBlock.BlockHeight, DateTime.Now, synchronizationConfirmedBlock.ReportedTime, synchronizationConfirmedBlock.RawData.ToArray());
						}

						if (item is SynchronizationRegistryCombinedBlock combinedBlock)
						{
							Logger.LogIfDebug(() => $"Adding to buffer combined block {combinedBlock.Height}");
							_bufferCombinedPackets.Post(combinedBlock);
							//Service.AddSynchronizationRegistryCombinedBlock(combinedBlock.BlockHeight, combinedBlock.SyncBlockHeight, combinedBlock.BlockHeight, combinedBlock.RawData.ToArray(), combinedBlock.BlockHashes);
						}
					}
				}
				catch (OperationCanceledException)
				{
				}
			}, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

		private async Task ConsumeSynchronizationConfirmedBlocks(IReceivableSourceBlock<SynchronizationConfirmedBlock> source, CancellationToken cancellationToken)
		{
			while (await source.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
			{
				try
				{
					if (source.TryReceiveAll(out IList<SynchronizationConfirmedBlock> blocks))
					{
						Logger.Debug($"Getting from buffer and storing bulk of {nameof(SynchronizationConfirmedBlock)} with heights {string.Join(',', blocks.Select(b => b.Height))}");
						Service.AddSynchronizationBlocks(blocks.Select(b => new SynchronizationBlock { SynchronizationBlockId = (long)b.Height, ReceiveTime = DateTime.Now, MedianTime = b.ReportedTime, BlockContent = b.RawData.ToArray() }).ToArray());
					}
					else
					{
						SynchronizationConfirmedBlock block = source.Receive(cancellationToken);
						Logger.Debug($"Getting from buffer and storing {nameof(SynchronizationConfirmedBlock)} with height {block.Height}");
						Service.AddSynchronizationBlock(block.Height, DateTime.Now, block.ReportedTime, block.RawData.ToArray());
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Failure at {nameof(ConsumeSynchronizationConfirmedBlocks)}", ex);
				}
			}
		}

		private async Task ConsumeSynchronizationRegistryCombinedBlock(IReceivableSourceBlock<SynchronizationRegistryCombinedBlock> source, CancellationToken cancellationToken)
		{
			while (await source.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
			{
				try
				{
					if (source.TryReceiveAll(out IList<SynchronizationRegistryCombinedBlock> blocks))
					{
						Logger.Debug($"Getting from buffer and storing bulk of {nameof(SynchronizationRegistryCombinedBlock)} with heights {string.Join(',', blocks.Select(b => b.Height))}");
						Service.AddSynchronizationRegistryCombinedBlocks(blocks.Select(b => new RegistryCombinedBlock { RegistryCombinedBlockId = (long)b.Height, SyncBlockHeight = b.SyncHeight, Content = b.RawData.ToArray(), FullBlockHashes = string.Join(",", b.BlockHashes.Select(h => h.ToHexString())) }).ToArray());
					}
					else
					{
						SynchronizationRegistryCombinedBlock block = source.Receive(cancellationToken);
						Logger.Debug($"Getting from buffer and storing {nameof(SynchronizationRegistryCombinedBlock)} with height {block.Height}");
						Service.AddSynchronizationRegistryCombinedBlock(block.Height, block.SyncHeight, block.RawData.ToArray(), block.BlockHashes);
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Failure at {nameof(ConsumeSynchronizationRegistryCombinedBlock)}", ex);
				}
			}
		}
	}
}
