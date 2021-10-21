using O10.Core.Persistency;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Node.DataLayer.Tests
{
    public class TestHelper
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();
        private readonly CancellationTokenSource _cancellationTokenSource;

        public TestHelper(string databaseConnectionString)
        {
            DatabaseConnectionString = databaseConnectionString
                .Replace(SqlServerDockerCollectionFixture.DATABASE_NAME_PLACEHOLDER, _databaseName);

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public string DatabaseConnectionString { get; }

        public async Task InitDataAccessService(IDataAccessService dataAccessService)
        {
            await dataAccessService.Initialize(_cancellationTokenSource.Token);
        }
    }
}
