using Microsoft.Data.Sqlite;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Core.Persistency.Configuration;
using System.Data;

namespace O10.Core.Persistency.SQLite
{
    [RegisterExtension(typeof(IDbConnectionProvider), Lifetime = LifetimeManagement.Singleton)]
    public class DbConnectionProvider : IDbConnectionProvider
    {
        private readonly IDataLayerConfiguration _dataLayerConfiguration;

        public DbConnectionProvider(IConfigurationService configurationService)
        {
            _dataLayerConfiguration = configurationService.Get<IDataLayerConfiguration>();
        }

        public string ConnectionType => "SQLite";

        public IDbConnection GetDbConnection()
        {
            return new SqliteConnection(_dataLayerConfiguration.ConnectionString);
        }
    }
}
