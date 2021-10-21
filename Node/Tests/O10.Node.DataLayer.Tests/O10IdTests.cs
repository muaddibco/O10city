using O10.Tests.Core.Fixtures;
using Xunit;
using O10.Node.DataLayer.Specific.O10Id;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.O10Id.DataContexts.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Persistency;

namespace O10.Node.DataLayer.Tests
{
    public class O10IdTests : DalTestBase
    {
        private readonly INodeDataContextRepository _dataContextRepository;

        public O10IdTests(SqlServerDockerCollectionFixture fixture, CoreFixture coreFixture) : base(fixture, coreFixture)
        {
        }

        protected override NodeDataContextBase GetNodeDataContext()
        {
            return new DataContext();
        }

        protected override void RegisterServices()
        {
            _coreFixture.ServiceCollection.AddSingleton<IDataAccessService, DataAccessService>();
        }

        [Fact]
        public void Test()
        {
        }
    }
}
