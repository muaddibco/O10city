using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using O10.Core.Configuration;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Core.Persistency;
using O10.Core.Persistency.Configuration;
using O10.Node.DataLayer.DataAccess;
using O10.Tests.Core.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions.Ordering;

namespace O10.Node.DataLayer.Tests
{
    public abstract class DalTestBase : IAsyncLifetime, IAssemblyFixture<SqlServerDockerCollectionFixture>, IClassFixture<CoreFixture>
    {
        protected readonly SqlServerDockerCollectionFixture _fixture;
        protected readonly CoreFixture _coreFixture;
        protected TestHelper _testHelper;
        protected IDataAccessService _dataAccessService;
        private NodeDataContextBase _nodeDataContext;

        protected DalTestBase(SqlServerDockerCollectionFixture fixture, CoreFixture coreFixture)
        {
            _fixture = fixture;
            _coreFixture = coreFixture;
        }

        public async Task DisposeAsync()
        {
            await _nodeDataContext.Database.EnsureDeletedAsync();
        }

        public async Task InitializeAsync()
        {
            var sqlConnectionString = _fixture.GetSqlConnectionString();
            _testHelper = new TestHelper(sqlConnectionString);

            var dataLayerConfiguration = Substitute.For<IDataLayerConfiguration>();
            dataLayerConfiguration.ConnectionString.ReturnsForAnyArgs(_testHelper.DatabaseConnectionString);
            dataLayerConfiguration.ConnectionType.ReturnsForAnyArgs("SqlServer");

            _coreFixture.ConfigurationService.Get<IDataLayerConfiguration>().ReturnsForAnyArgs(dataLayerConfiguration);

            _nodeDataContext = GetNodeDataContext();

            var dataContextRepository = Substitute.For<INodeDataContextRepository>();
            dataContextRepository.GetInstance(Transactions.Core.Enums.LedgerType.O10State, "SqlServer").ReturnsForAnyArgs(_nodeDataContext);

            _coreFixture.ServiceCollection.AddSingleton(dataContextRepository);

            RegisterServices();

            _coreFixture.BuildContainer();

            _dataAccessService = _coreFixture.ServiceProvider.GetRequiredService<IDataAccessService>();

            await _testHelper.InitDataAccessService(_dataAccessService);
        }

        protected abstract NodeDataContextBase GetNodeDataContext();

        protected abstract void RegisterServices();
    }
}
