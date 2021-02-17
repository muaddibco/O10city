using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.Registry.DataContexts;
using O10.Node.DataLayer.Specific.Registry.Model;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Core.Tracking;
using O10.Core.DataLayer;

namespace O10.Node.DataLayer.Specific.Registry
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessService : NodeDataAccessServiceBase<RegistryDataContextBase>
    {
        public DataAccessService(INodeDataContextRepository dataContextRepository,
                                 IConfigurationService configurationService,
                                 ITrackingService trackingService,
                                 ILoggerService loggerService)
                : base(dataContextRepository, configurationService, trackingService, loggerService)
        {
        }

        public override LedgerType PacketType => LedgerType.Registry;

        public void AddRegistryFullBlock(ulong syncBlockHeight, ulong round, int transactionsCount, byte[] content, byte[] hash)
        {
            RegistryFullBlock registryFullBlock = new RegistryFullBlock
            {
                SyncBlockHeight = syncBlockHeight,
                Round = round,
                TransactionsCount = transactionsCount,
                Content = content,
                Hash = hash.ToHexString(),
                HashString = hash.ToHexString()
            };

            lock (Sync)
            {
                DataContext.RegistryFullBlocks.Add(registryFullBlock);
            }
        }

        public void AddRegistryFullBlocks(RegistryFullBlock[] blocks)
        {
            lock (Sync)
            {
                DateTimeOffset start = DateTimeOffset.UtcNow;
                Stopwatch stopwatch = Stopwatch.StartNew();
                DataContext.RegistryFullBlocks.AddRange(blocks);
                stopwatch.Stop();
                TrackingService.TrackDependency(nameof(DataAccessService), nameof(AddRegistryFullBlocks), blocks?.Length.ToString(CultureInfo.InvariantCulture), start, stopwatch.Elapsed);
            }
        }

        [Obsolete("not in use, all registry blocks are preloaded into dictionary")]
        public List<RegistryFullBlock> GetRegistryFullBlocks(ulong syncBlockHeight)
        {
            lock (Sync)
            {
                DateTimeOffset start = DateTimeOffset.UtcNow;
                Stopwatch stopwatch = Stopwatch.StartNew();
                List<RegistryFullBlock> registryFullBlocks = DataContext.RegistryFullBlocks.Where(r => r.SyncBlockHeight == syncBlockHeight).ToList();
                stopwatch.Stop();
                TrackingService.TrackDependency(nameof(DataAccessService), nameof(GetRegistryFullBlocks), syncBlockHeight.ToString(CultureInfo.InvariantCulture), start, stopwatch.Elapsed);
                return registryFullBlocks;
            }
        }

        public IEnumerable<RegistryFullBlock> GetAllRegistryFullBlocks()
        {
            lock (Sync)
            {
                return DataContext.RegistryFullBlocks;
            }
        }
    }
}
