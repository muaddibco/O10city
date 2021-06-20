using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
using O10.Core.Models;
using O10.Core.Notifications;
using O10.Transactions.Core.Ledgers.Synchronization.Transactions;
using AggregatedRegistrationsTransactionDb = O10.Node.DataLayer.Specific.Synchronization.Model.AggregatedRegistrationsTransaction;
using SynchronizationPacketDb = O10.Node.DataLayer.Specific.Synchronization.Model.SynchronizationPacket;
using O10.Transactions.Core.Ledgers.Synchronization;
using O10.Transactions.Core.Ledgers.Registry;

namespace O10.Node.DataLayer.Specific.Synchronization
{
    [RegisterExtension(typeof(IChainDataService), Lifetime = LifetimeManagement.Singleton)]
    public class DataService : ChainDataServiceBase<DataAccessService>
	{
		private readonly BlockingCollection<TaskCompletionWrapper<IPacketBase>> _packets = new BlockingCollection<TaskCompletionWrapper<IPacketBase>>();
		private BufferBlock<TaskCompletionWrapper<IPacketBase>> _bufferSyncConfirmedPackets;
		private BufferBlock<TaskCompletionWrapper<IPacketBase>> _bufferCombinedPackets;

		public DataService(
            INodeDataAccessServiceRepository dataAccessServiceRepository,
            ITranslatorsRepository translatorsRepository,
            IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
            ILoggerService loggerService)
			: base(dataAccessServiceRepository, translatorsRepository, identityKeyProvidersRegistry, loggerService)
        {
        }

        override public LedgerType LedgerType => LedgerType.Synchronization;

		override public TaskCompletionWrapper<IPacketBase> Add(IPacketBase item)
        {
			var wrapperOfPacket = new TaskCompletionWrapper<IPacketBase>(item);
			_packets.Add(wrapperOfPacket);

			TaskCompletionWrapper<IPacketBase> wrapperOfKey = new TaskCompletionWrapper<IPacketBase>(item);

			wrapperOfPacket.TaskCompletion.Task.ContinueWith((t, o) =>
			{
				if(t.IsCompletedSuccessfully)
                {
					((TaskCompletionWrapper<IPacketBase>)o).TaskCompletion.SetResult(new SucceededNotification());
                }
				else
                {
					((TaskCompletionWrapper<IPacketBase>)o).TaskCompletion.SetException(t.Exception);

				}
			}, wrapperOfKey, TaskScheduler.Default);

			return wrapperOfKey;
        }

        public override long GetScalar(IDataKey key)
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

        override public IEnumerable<IPacketBase> Get(IDataKey key)
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
                            AggregatedRegistrationsTransactionDb block = Service.GetLastRegistryCombinedBlock();
							return new List<IPacketBase> { TranslatorsRepository.GetInstance<AggregatedRegistrationsTransactionDb, IPacketBase>().Translate(block) };
						}
					case TransactionTypes.Synchronization_ConfirmedBlock:
						{
                            SynchronizationPacketDb block = Service.GetLastSynchronizationBlock();
							return new List<IPacketBase> { TranslatorsRepository.GetInstance<SynchronizationPacketDb, IPacketBase>().Translate(block) };
						}
					default:
						return null;
				}
			}
			else if (key is BlockTypeLowHeightKey blockTypeLowHeightKey)
			{
				if (blockTypeLowHeightKey.BlockType == TransactionTypes.Synchronization_ConfirmedBlock)
				{
					return Service.GetAllLastSynchronizationBlocks(blockTypeLowHeightKey.Height).Select(b => TranslatorsRepository.GetInstance<SynchronizationPacketDb, IPacketBase>().Translate(b));
				}
				else if (blockTypeLowHeightKey.BlockType == TransactionTypes.Synchronization_RegistryCombinationBlock)
				{
					return Service.GetAllLastRegistryCombinedBlocks(blockTypeLowHeightKey.Height).OrderBy(b => b.AggregatedRegistrationsTransactionId).Select(b => TranslatorsRepository.GetInstance<AggregatedRegistrationsTransactionDb, IPacketBase>().Translate(b));
				}
			}
			else if (key is BlockTypeKey blockTypeKey)
			{
				if (blockTypeKey.BlockType == TransactionTypes.Synchronization_ConfirmedBlock)
				{
					return Service.GetAllSynchronizationBlocks().Select(b => TranslatorsRepository.GetInstance<SynchronizationPacketDb, IPacketBase>().Translate(b));
				}
				else if (blockTypeKey.BlockType == TransactionTypes.Synchronization_RegistryCombinationBlock)
				{
					return Service.GetAllRegistryCombinedBlocks().Select(b => TranslatorsRepository.GetInstance<AggregatedRegistrationsTransactionDb, IPacketBase>().Translate(b));
				}
			}

			throw new DataKeyNotSupportedException(key);
		}

		override public void Initialize(CancellationToken cancellationToken)
        {
			_bufferSyncConfirmedPackets = new BufferBlock<TaskCompletionWrapper<IPacketBase>>(new DataflowBlockOptions { CancellationToken = cancellationToken });
			_bufferCombinedPackets = new BufferBlock<TaskCompletionWrapper<IPacketBase>>(new DataflowBlockOptions { CancellationToken = cancellationToken });

			ConsumeSynchronizationConfirmedBlocks(_bufferSyncConfirmedPackets, cancellationToken);
			ConsumeSynchronizationRegistryCombinedBlock(_bufferCombinedPackets, cancellationToken);

            Task.Factory.StartNew(() =>
			{
				try
				{
					foreach (var item in _packets.GetConsumingEnumerable(cancellationToken))
					{
						if (item.State.Transaction is SynchronizationConfirmedTransaction synchronizationConfirmed)
						{
                            Logger.LogIfDebug(() => $"Adding to buffer synchronization block {item.State.AsPacket<SynchronizationPacket>().Payload.Height}");
                            _bufferSyncConfirmedPackets.Post(item);
						}

						if (item.State.Transaction is AggregatedRegistrationsTransaction aggregatedRegistrations)
						{
                            Logger.LogIfDebug(() => $"Adding to buffer combined block {item.State.AsPacket<SynchronizationPacket>().Payload.Height}");
                            _bufferCombinedPackets.Post(item);
						}
					}
				}
				catch (OperationCanceledException)
				{
				}
			}, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

		private async Task ConsumeSynchronizationConfirmedBlocks(IReceivableSourceBlock<TaskCompletionWrapper<IPacketBase>> source, CancellationToken cancellationToken)
		{
			while (await source.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
			{
				try
				{
					if (source.TryReceiveAll(out IList<TaskCompletionWrapper<IPacketBase>> packets))
					{
						Logger.Debug($"Getting from buffer and storing bulk of {nameof(SynchronizationConfirmedTransaction)} with heights {string.Join(',', packets.Select(b => ((SynchronizationPacket)b.State).Payload.Height))}");
						Service.AddSynchronizationBlocks(packets.Select(b => new SynchronizationPacketDb { SynchronizationPacketId = ((SynchronizationPacket)b.State).Payload.Height, ReceiveTime = DateTime.Now, MedianTime = ((SynchronizationPacket)b.State).Payload.ReportedTime, Content = b.ToString() }).ToArray());
					}
					else
					{
						var wrapper = source.Receive(cancellationToken);
						Logger.Debug($"Getting from buffer and storing {nameof(SynchronizationConfirmedTransaction)} with height {wrapper.State.AsPacket<SynchronizationPacket>().Payload.Height}");
						Service.AddSynchronizationBlock(wrapper.State.AsPacket<SynchronizationPacket>().Payload.Height, DateTime.Now, wrapper.State.AsPacket<SynchronizationPacket>().Payload.ReportedTime, wrapper.State.ToString());
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Failure at {nameof(ConsumeSynchronizationConfirmedBlocks)}", ex);
				}
			}
		}

		private async Task ConsumeSynchronizationRegistryCombinedBlock(IReceivableSourceBlock<TaskCompletionWrapper<IPacketBase>> source, CancellationToken cancellationToken)
		{
			while (await source.OutputAvailableAsync(cancellationToken).ConfigureAwait(false))
			{
				try
				{
					if (source.TryReceiveAll(out IList<TaskCompletionWrapper<IPacketBase>> wrappers))
					{
						Logger.Debug($"Getting from buffer and storing bulk of {nameof(AggregatedRegistrationsTransaction)} with heights {string.Join(',', wrappers.Cast<SynchronizationPacket>().Select(b => b.Payload.Height))}");
                        Service.AddSynchronizationRegistryCombinedBlocks(wrappers.Cast<SynchronizationPacket>().Select(b => new AggregatedRegistrationsTransactionDb { AggregatedRegistrationsTransactionId = b.Payload.Height, SyncBlockHeight = b.With<AggregatedRegistrationsTransaction>().SyncHeight, Content = b.ToString(), FullBlockHashes = string.Join(",", b.With<AggregatedRegistrationsTransaction>().BlockHashes.Select(h => h.ToHexString())) }).ToArray());
					}
					else
					{
						var wrapper = source.Receive(cancellationToken);
						var transaction = ((SynchronizationPacket)wrapper.State).With<AggregatedRegistrationsTransaction>();
						Logger.Debug($"Getting from buffer and storing {nameof(AggregatedRegistrationsTransaction)} with height {wrapper.State.AsPacket<SynchronizationPacket>().Payload.Height}");
						Service.AddSynchronizationRegistryCombinedBlock(wrapper.State.AsPacket<SynchronizationPacket>().Payload.Height, transaction.SyncHeight, wrapper.State.ToString(), transaction.BlockHashes.Select(h => h.ToHexString()).ToArray());
					}
				}
				catch (Exception ex)
				{
					Logger.Error($"Failure at {nameof(ConsumeSynchronizationRegistryCombinedBlock)}", ex);
				}
			}
		}

        public override void AddDataKey(IDataKey key, IDataKey newKey)
        {
            throw new NotImplementedException();
        }
    }
}
