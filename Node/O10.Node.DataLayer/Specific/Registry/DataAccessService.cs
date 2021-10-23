using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.Registry.DataContexts;
using O10.Node.DataLayer.Specific.Registry.Model;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Core.Persistency;
using System.Threading.Tasks;
using System.Threading;

namespace O10.Node.DataLayer.Specific.Registry
{
    [RegisterExtension(typeof(IDataAccessService), Lifetime = LifetimeManagement.Scoped)]
    public class DataAccessService : NodeDataAccessServiceBase<RegistryDataContextBase>
    {
        public DataAccessService(INodeDataContextRepository dataContextRepository, 
                                 IConfigurationService configurationService,
                                 ILoggerService loggerService)
                : base(dataContextRepository, configurationService, loggerService)
        {
        }

        public override LedgerType LedgerType => LedgerType.Registry;

        public async Task AddRegistryFullBlocks(RegistryFullBlock[] blocks, CancellationToken cancellationToken = default)
        {
            DataContext.RegistryFullBlocks.AddRange(blocks);
            await DataContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IEnumerable<RegistryFullBlock>> GetAllRegistryFullBlocks()
        {
            string sql = "SELECT * FROM RegistryFullBlocks";

            return await DataContext.QueryAsync<RegistryFullBlock>(sql, cancellationToken: CancellationToken);
        }
    }
}
