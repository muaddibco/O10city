using System;
using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.Synchronization.DataContexts;
using O10.Node.DataLayer.Specific.Synchronization.Model;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Core.Persistency;
using System.Threading.Tasks;
using System.Threading;

namespace O10.Node.DataLayer.Specific.Synchronization
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Scoped)]
    public class DataAccessService : NodeDataAccessServiceBase<SynchronizationDataContextBase>
    {
        public DataAccessService(INodeDataContextRepository dataContextRepository, 
                                 IConfigurationService configurationService,
                                 ILoggerService loggerService)
            : base(dataContextRepository, configurationService, loggerService)
        {
        }

        public override LedgerType LedgerType => LedgerType.Synchronization;

        #region Synchronization

        public async Task AddSynchronizationBlock(long height, DateTime receiveTime, DateTime medianTime, string content, CancellationToken cancellationToken = default)
        {
            using var dbContext = GetDataContext();
            dbContext.SynchronizationBlocks.Add(new SynchronizationPacket
            {
                SynchronizationPacketId = height,
                ReceiveTime = receiveTime,
                MedianTime = medianTime,
                Content = content
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddSynchronizationBlocks(SynchronizationPacket[] blocks, CancellationToken cancellationToken = default)
        {
            using var dbContext = GetDataContext();
            dbContext.SynchronizationBlocks.AddRange(blocks);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<SynchronizationPacket> GetLastSynchronizationBlock(CancellationToken cancellationToken)
        {
            string sql = "SELECT TOP 1 * FROM SynchronizationPackets ORDER BY SynchronizationPacketId DESC";
            return await DataContext.QueryFirstOrDefaultAsync<SynchronizationPacket>(sql, cancellationToken: cancellationToken);
        }

        public IEnumerable<SynchronizationPacket> GetAllLastSynchronizationBlocks(ulong height)
        {
            using var dbContext = GetDataContext();

            //TODO: change back to queriable
            List<SynchronizationPacket> lastSyncBlocks = dbContext.SynchronizationBlocks.OrderByDescending(b => b.SynchronizationPacketId).Where(b => b.SynchronizationPacketId > (long)height).ToList();
            return lastSyncBlocks;
        }

        public IEnumerable<SynchronizationPacket> GetAllSynchronizationBlocks()
        {
            using var dbContext = GetDataContext();

            return dbContext.SynchronizationBlocks.ToList();
        }

        public IEnumerable<AggregatedRegistrationsTransaction> GetAllRegistryCombinedBlocks()
        {
            using var dbContext = GetDataContext();

            return dbContext.RegistryCombinedBlocks.ToList();
        }

        public IEnumerable<AggregatedRegistrationsTransaction> GetAllLastRegistryCombinedBlocks(ulong height)
        {
            using var dbContext = GetDataContext();

            return dbContext.RegistryCombinedBlocks.Where(b => b.AggregatedRegistrationsTransactionId > (long)height).OrderByDescending(b => b.AggregatedRegistrationsTransactionId).ToList();
        }

        public async Task AddSynchronizationRegistryCombinedBlock(long height, long syncBlockHeight, string content, string[] hashes, CancellationToken cancellationToken = default)
        {
            Logger.LogIfDebug(() => $"Storing RegistryCombinedBlock with height {height}");
            using var dbContext = GetDataContext();

            dbContext.RegistryCombinedBlocks.Add(new AggregatedRegistrationsTransaction
            {
                AggregatedRegistrationsTransactionId = height,
                SyncBlockHeight = syncBlockHeight,
                Content = content,
                FullBlockHashes = string.Join(",", hashes)
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task AddSynchronizationRegistryCombinedBlocks(AggregatedRegistrationsTransaction[] blocks, CancellationToken cancellationToken = default)
        {
            using var dbContext = GetDataContext();

            Logger.LogIfDebug(() => $"Storing bulk of RegistryCombinedBlock with heights {string.Join(',', blocks.Select(b => b.AggregatedRegistrationsTransactionId))}");
            dbContext.RegistryCombinedBlocks.AddRange(blocks);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<AggregatedRegistrationsTransaction> GetLastRegistryCombinedBlock(CancellationToken cancellationToken)
        {
            string sql = "SELECT TOP 1 * FROM AggregatedRegistrationsTransactions ORDER BY AggregatedRegistrationsTransactionId DESC";

            return await DataContext.QueryFirstOrDefaultAsync<AggregatedRegistrationsTransaction>(sql, cancellationToken: cancellationToken);
        }

        public AggregatedRegistrationsTransaction GetRegistryCombinedBlockByHeight(ulong combinedBlockHeight)
        {
            using var dbContext = GetDataContext();
            return GetLocalAwareRegistryCombinedBlock(dbContext, combinedBlockHeight);
        }

        private AggregatedRegistrationsTransaction GetLocalAwareRegistryCombinedBlock(SynchronizationDataContextBase dbContext, ulong combinedBlockHeight) =>
            dbContext.RegistryCombinedBlocks.Local.FirstOrDefault(c => c.AggregatedRegistrationsTransactionId == (long)combinedBlockHeight)
                ?? dbContext.RegistryCombinedBlocks.FirstOrDefault(c => c.AggregatedRegistrationsTransactionId == (long)combinedBlockHeight);

        #endregion Synchronization
    }
}
