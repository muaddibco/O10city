using O10.Tests.Core.Fixtures;
using Xunit;
using O10.Node.DataLayer.Specific.O10Id;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.O10Id.DataContexts.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Persistency;
using O10.Crypto.ConfidentialAssets;
using O10.Core.Identity;
using O10.Core.ExtensionMethods;
using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using O10.Node.DataLayer.Specific.O10Id.Model;

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
        public async Task Test()
        {
            var dataAccessService = _coreFixture.ServiceProvider.GetRequiredService<IDataAccessService>() as DataAccessService;

            string content = Guid.NewGuid().ToString();
            byte[] hash = CryptoHelper.GetRandomSeed();
            byte[] source = CryptoHelper.GetRandomSeed();
            IKey sourceKey = _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(source);

            var transaction = await dataAccessService.AddTransaction(sourceKey, 1, 1, content, hash, CancellationToken.None);

            transaction
                .Should()
                .NotBeNull()
                .And.Match<O10Transaction>(t => t.Source != null && t.Source.Identity != null && t.Source.Identity.PublicKey.Equals32(source))
                .And.Match<O10Transaction>(t => t.PacketType == 1)
                .And.Match<O10Transaction>(t => t.Height == 1)
                .And.Subject.As<O10Transaction>().Content.Should().Be(content);
        }
    }
}
