using System.Threading.Tasks;
using Xunit;
namespace O10.Node.DataLayer.Tests
{
    public class SqlServerDockerCollectionFixture : IAsyncLifetime
    {
        public const string DATABASE_NAME_PLACEHOLDER = "@@databaseName@@";
        private string _dockerContainerId;
        private string _dockerSqlPort;

        public string GetSqlConnectionString()
        {
            return $"Data Source=localhost,{_dockerSqlPort};" +
                $"Initial Catalog={DATABASE_NAME_PLACEHOLDER};" +
                "Integrated Security=False;" +
                "User ID=SA;" +
                $"Password={DockerSqlDatabaseUtilities.SQLSERVER_SA_PASSWORD}";
        }

        public async Task InitializeAsync()
        {
            (_dockerContainerId, _dockerSqlPort) = await DockerSqlDatabaseUtilities.EnsureDockerStartedAndGetContainerIdAndPortAsync();
        }

        public Task DisposeAsync()
        {
            return DockerSqlDatabaseUtilities.EnsureDockerStoppedAndRemovedAsync(_dockerContainerId);
        }
    }
}
