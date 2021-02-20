using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.Synchronization.DataContexts;
using O10.Node.DataLayer.Specific.Synchronization.Model;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Core.Tracking;
using O10.Core.DataLayer;

namespace O10.Node.DataLayer.Specific.Synchronization
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
	public class DataAccessService : NodeDataAccessServiceBase<SynchronizationDataContextBase>
    {
        public DataAccessService(INodeDataContextRepository dataContextRepository,
                                 IConfigurationService configurationService,
                                 ITrackingService trackingService,
                                 ILoggerService loggerService)
            : base(dataContextRepository, configurationService, trackingService, loggerService)
        {
        }

        public override LedgerType LedgerType => LedgerType.Synchronization;

        #region Synchronization

        public void AddSynchronizationBlock(ulong blockHeight, DateTime receiveTime, DateTime medianTime, byte[] content)
        {
            lock (Sync)
            {
                DataContext.SynchronizationBlocks.Add(new SynchronizationBlock
                {
                    SynchronizationBlockId = (long)blockHeight,
                    ReceiveTime = receiveTime,
                    MedianTime = medianTime,
                    BlockContent = content
                });
            }
        }

		public void AddSynchronizationBlocks(SynchronizationBlock[] blocks)
		{
			lock (Sync)
			{
				DataContext.SynchronizationBlocks.AddRange(blocks);
			}
		}

		public SynchronizationBlock GetLastSynchronizationBlock()
        {
            lock (Sync)
            {
                return DataContext.SynchronizationBlocks.OrderByDescending(b => b.SynchronizationBlockId).FirstOrDefault();
            }
        }

        public IEnumerable<SynchronizationBlock> GetAllLastSynchronizationBlocks(ulong height)
        {
            lock (Sync)
            {
                DateTimeOffset start = DateTimeOffset.UtcNow;
                Stopwatch stopwatch = Stopwatch.StartNew();
                //TODO: change back to queriable
                List<SynchronizationBlock> lastSyncBlocks = DataContext.SynchronizationBlocks.OrderByDescending(b => b.SynchronizationBlockId).Where(b => b.SynchronizationBlockId > (long)height).ToList();
                stopwatch.Stop();
                TrackingService.TrackDependency(nameof(DataAccessService), nameof(GetAllLastSynchronizationBlocks), $"height: {height.ToString(CultureInfo.InvariantCulture)}", start, stopwatch.Elapsed);
                return lastSyncBlocks;
            }
        }

        public IEnumerable<SynchronizationBlock> GetAllSynchronizationBlocks()
        {
            lock (Sync)
            {
                return DataContext.SynchronizationBlocks.ToList();
            }
        }

        public IEnumerable<RegistryCombinedBlock> GetAllRegistryCombinedBlocks()
        {
            lock (Sync)
            {
                return DataContext.RegistryCombinedBlocks.ToList();
            }
        }

        public IEnumerable<RegistryCombinedBlock> GetAllLastRegistryCombinedBlocks(ulong height)
        {
            lock (Sync)
            {
                DateTimeOffset start = DateTimeOffset.UtcNow;
                Stopwatch stopwatch = Stopwatch.StartNew();
                var registryCombinedBlocks = DataContext.RegistryCombinedBlocks.Where(b => b.RegistryCombinedBlockId > (long)height).OrderByDescending(b => b.RegistryCombinedBlockId).ToList();
                stopwatch.Stop();
                TrackingService.TrackDependency(nameof(DataAccessService), nameof(GetAllLastRegistryCombinedBlocks), $"{nameof(height)}: {height.ToString(CultureInfo.InvariantCulture)}", start, stopwatch.Elapsed);
                return registryCombinedBlocks;
            }
        }

        public void AddSynchronizationRegistryCombinedBlock(ulong blockHeight, ulong syncBlockHeight, byte[] content, byte[][] hashes)
        {
            Logger.LogIfDebug(() => $"Storing RegistryCombinedBlock with height {blockHeight}");
            lock (Sync)
            {
				DataContext.RegistryCombinedBlocks.Add(new RegistryCombinedBlock
				{
					RegistryCombinedBlockId = (long)blockHeight,
					SyncBlockHeight = syncBlockHeight,
					Content = content,
					FullBlockHashes = string.Join(",", hashes.Select(h => h.ToHexString()))
                });
            }
        }

		public void AddSynchronizationRegistryCombinedBlocks(RegistryCombinedBlock[] blocks)
		{
			lock (Sync)
			{
                Logger.LogIfDebug(() => $"Storing bulk of RegistryCombinedBlock with heights {string.Join(',', blocks.Select(b => b.RegistryCombinedBlockId))}");
				DataContext.RegistryCombinedBlocks.AddRange(blocks);
			}
		}

		public RegistryCombinedBlock GetLastRegistryCombinedBlock()
        {
            lock (Sync)
            {
                return DataContext.RegistryCombinedBlocks.OrderByDescending(b => b.RegistryCombinedBlockId).FirstOrDefault();
            }
        }

        public RegistryCombinedBlock GetRegistryCombinedBlockByHeight(ulong combinedBlockHeight) => GetLocalAwareRegistryCombinedBlock(combinedBlockHeight);

        private RegistryCombinedBlock GetLocalAwareRegistryCombinedBlock(ulong combinedBlockHeight) =>
            DataContext.RegistryCombinedBlocks.Local.FirstOrDefault(c => c.RegistryCombinedBlockId == (long)combinedBlockHeight)
                ?? DataContext.RegistryCombinedBlocks.FirstOrDefault(c => c.RegistryCombinedBlockId == (long)combinedBlockHeight);

        #endregion Synchronization
    }
}
