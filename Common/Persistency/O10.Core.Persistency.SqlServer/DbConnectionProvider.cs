using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Persistency.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace O10.Core.Persistency.SqlServer
{
    [RegisterExtension(typeof(IDbConnectionProvider), Lifetime = LifetimeManagement.Singleton)]
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly IDataLayerConfiguration _dataLayerConfiguration;

        public DbConnectionProvider(IConfigurationService configurationService)
        {
            _dataLayerConfiguration = configurationService.Get<IDataLayerConfiguration>();
        }

        public string ConnectionType => "SqlServer";

        public IDbConnection GetDbConnection()
        {
            return new SqlConnection(_dataLayerConfiguration.ConnectionString);
        }
    }
}
