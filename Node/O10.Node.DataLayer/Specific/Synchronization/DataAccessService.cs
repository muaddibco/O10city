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
            string sql = "INSERT SynchronizationPackets(SynchronizationPacketId, ReceiveTime, MedianTime, Content) VALUES(@SynchronizationPacketId, @ReceiveTime, @MedianTime, @Content)";

            await DataContext.ExecuteAsync(
                sql,
                new
                {
                    SynchronizationPacketId = height,
                    ReceiveTime = receiveTime,
                    MedianTime = medianTime,
                    Content = content
                },
                cancellationToken: cancellationToken);
        }

        public async Task AddSynchronizationBlocks(SynchronizationPacket[] blocks, CancellationToken cancellationToken = default)
        {
            DataContext.SynchronizationBlocks.AddRange(blocks);

            await DataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<SynchronizationPacket> GetLastSynchronizationBlock(CancellationToken cancellationToken)
        {
            string sql = "SELECT TOP 1 * FROM SynchronizationPackets ORDER BY SynchronizationPacketId DESC";
            return await DataContext.QueryFirstOrDefaultAsync<SynchronizationPacket>(sql, cancellationToken: cancellationToken);
        }

        public async Task<IEnumerable<SynchronizationPacket>> GetAllLastSynchronizationBlocks(ulong height, CancellationToken cancellationToken)
        {
            string sql = "SELECT * FROM SynchronizationPackets WHERE SynchronizationPacketId>@Height ORDER BY SynchronizationPacketId DESC";
            return await DataContext.QueryAsync<SynchronizationPacket>(
                sql,
                new
                {
                    Height = (long)height
                },
                cancellationToken: cancellationToken);
        }

        public IEnumerable<SynchronizationPacket> GetAllSynchronizationBlocks()
        {
            return DataContext.SynchronizationBlocks.ToList();
        }

        public IEnumerable<AggregatedRegistrationsTransaction> GetAllRegistryCombinedBlocks()
        {
            return DataContext.RegistryCombinedBlocks.ToList();
        }

        public async Task<IEnumerable<AggregatedRegistrationsTransaction>> GetAllLastRegistryCombinedBlocks(ulong height, CancellationToken cancellationToken)
        {
            string sql = "SELECT * FROM AggregatedRegistrationsTransactions WHERE AggregatedRegistrationsTransactionId>@Height ORDER BY AggregatedRegistrationsTransactionId DESC";
            return await DataContext.QueryAsync<AggregatedRegistrationsTransaction>(
                sql,
                new
                {
                    Height = (long)height
                },
                cancellationToken: cancellationToken);
        }

        public async Task AddSynchronizationRegistryCombinedBlock(long height, long syncBlockHeight, string content, string[] hashes, CancellationToken cancellationToken = default)
        {
            Logger.LogIfDebug(() => $"Storing RegistryCombinedBlock with height {height}");

            string sql = "INSERT AggregatedRegistrationsTransactions(AggregatedRegistrationsTransactionId, SyncBlockHeight, Content, FullBlockHashes) VALUES(@AggregatedRegistrationsTransactionId, @SyncBlockHeight, @Content, @FullBlockHashes)";

            await DataContext.ExecuteAsync(
                sql, 
                new {
                    AggregatedRegistrationsTransactionId = height,
                    SyncBlockHeight = syncBlockHeight,
                    Content = content,
                    FullBlockHashes = string.Join(",", hashes)
                },
                cancellationToken: cancellationToken);
        }

        public async Task AddSynchronizationRegistryCombinedBlocks(AggregatedRegistrationsTransaction[] blocks, CancellationToken cancellationToken = default)
        {
            Logger.LogIfDebug(() => $"Storing bulk of RegistryCombinedBlock with heights {string.Join(',', blocks.Select(b => b.AggregatedRegistrationsTransactionId))}");
            DataContext.RegistryCombinedBlocks.AddRange(blocks);

            await DataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<AggregatedRegistrationsTransaction> GetLastRegistryCombinedBlock(CancellationToken cancellationToken)
        {
            string sql = "SELECT TOP 1 * FROM AggregatedRegistrationsTransactions ORDER BY AggregatedRegistrationsTransactionId DESC";

            return await DataContext.QueryFirstOrDefaultAsync<AggregatedRegistrationsTransaction>(sql, cancellationToken: cancellationToken);
        }

        #endregion Synchronization
    }
}
