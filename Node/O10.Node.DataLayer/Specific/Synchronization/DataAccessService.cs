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

        public void AddSynchronizationBlock(long height, DateTime receiveTime, DateTime medianTime, string content)
        {
            lock (Sync)
            {
                DataContext.SynchronizationBlocks.Add(new SynchronizationPacket
                {
                    SynchronizationPacketId = height,
                    ReceiveTime = receiveTime,
                    MedianTime = medianTime,
                    Content = content
                });
            }
        }

		public void AddSynchronizationBlocks(SynchronizationPacket[] blocks)
		{
			lock (Sync)
			{
				DataContext.SynchronizationBlocks.AddRange(blocks);
			}
		}

		public SynchronizationPacket GetLastSynchronizationBlock()
        {
            lock (Sync)
            {
                return DataContext.SynchronizationBlocks.OrderByDescending(b => b.SynchronizationPacketId).FirstOrDefault();
            }
        }

        public IEnumerable<SynchronizationPacket> GetAllLastSynchronizationBlocks(ulong height)
        {
            lock (Sync)
            {
                DateTimeOffset start = DateTimeOffset.UtcNow;
                Stopwatch stopwatch = Stopwatch.StartNew();
                //TODO: change back to queriable
                List<SynchronizationPacket> lastSyncBlocks = DataContext.SynchronizationBlocks.OrderByDescending(b => b.SynchronizationPacketId).Where(b => b.SynchronizationPacketId > (long)height).ToList();
                stopwatch.Stop();
                TrackingService.TrackDependency(nameof(DataAccessService), nameof(GetAllLastSynchronizationBlocks), $"height: {height.ToString(CultureInfo.InvariantCulture)}", start, stopwatch.Elapsed);
                return lastSyncBlocks;
            }
        }

        public IEnumerable<SynchronizationPacket> GetAllSynchronizationBlocks()
        {
            lock (Sync)
            {
                return DataContext.SynchronizationBlocks.ToList();
            }
        }

        public IEnumerable<AggregatedRegistrationsTransaction> GetAllRegistryCombinedBlocks()
        {
            lock (Sync)
            {
                return DataContext.RegistryCombinedBlocks.ToList();
            }
        }

        public IEnumerable<AggregatedRegistrationsTransaction> GetAllLastRegistryCombinedBlocks(ulong height)
        {
            lock (Sync)
            {
                DateTimeOffset start = DateTimeOffset.UtcNow;
                Stopwatch stopwatch = Stopwatch.StartNew();
                var registryCombinedBlocks = DataContext.RegistryCombinedBlocks.Where(b => b.AggregatedRegistrationsTransactionId > (long)height).OrderByDescending(b => b.AggregatedRegistrationsTransactionId).ToList();
                stopwatch.Stop();
                TrackingService.TrackDependency(nameof(DataAccessService), nameof(GetAllLastRegistryCombinedBlocks), $"{nameof(height)}: {height.ToString(CultureInfo.InvariantCulture)}", start, stopwatch.Elapsed);
                return registryCombinedBlocks;
            }
        }

        public void AddSynchronizationRegistryCombinedBlock(long height, long syncBlockHeight, string content, string[] hashes)
        {
            Logger.LogIfDebug(() => $"Storing RegistryCombinedBlock with height {height}");
            lock (Sync)
            {
				DataContext.RegistryCombinedBlocks.Add(new AggregatedRegistrationsTransaction
				{
					AggregatedRegistrationsTransactionId = height,
					SyncBlockHeight = syncBlockHeight,
					Content = content,
					FullBlockHashes = string.Join(",", hashes)
                });
            }
        }

		public void AddSynchronizationRegistryCombinedBlocks(AggregatedRegistrationsTransaction[] blocks)
		{
			lock (Sync)
			{
                Logger.LogIfDebug(() => $"Storing bulk of RegistryCombinedBlock with heights {string.Join(',', blocks.Select(b => b.AggregatedRegistrationsTransactionId))}");
				DataContext.RegistryCombinedBlocks.AddRange(blocks);
			}
		}

		public AggregatedRegistrationsTransaction GetLastRegistryCombinedBlock()
        {
            lock (Sync)
            {
                return DataContext.RegistryCombinedBlocks.OrderByDescending(b => b.AggregatedRegistrationsTransactionId).FirstOrDefault();
            }
        }

        public AggregatedRegistrationsTransaction GetRegistryCombinedBlockByHeight(ulong combinedBlockHeight) => GetLocalAwareRegistryCombinedBlock(combinedBlockHeight);

        private AggregatedRegistrationsTransaction GetLocalAwareRegistryCombinedBlock(ulong combinedBlockHeight) =>
            DataContext.RegistryCombinedBlocks.Local.FirstOrDefault(c => c.AggregatedRegistrationsTransactionId == (long)combinedBlockHeight)
                ?? DataContext.RegistryCombinedBlocks.FirstOrDefault(c => c.AggregatedRegistrationsTransactionId == (long)combinedBlockHeight);

        #endregion Synchronization
    }
}
