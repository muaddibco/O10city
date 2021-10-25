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
        private readonly DataContext _dataContext;

        public O10IdTests(SqlServerDockerCollectionFixture fixture, CoreFixture coreFixture) : base(fixture, coreFixture) => _dataContext = new DataContext();

        protected override NodeDataContextBase GetNodeDataContext() => _dataContext;

        protected override void RegisterServices()
        {
            _coreFixture.ServiceCollection.AddSingleton<IDataAccessService, DataAccessService>();
        }

        [Fact]
        public async Task AddTransactionTest()
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
                .And.Match<O10Transaction>(t => t.HashKey != null && t.HashKey.Hash.Equals32(hash))
                .And.Subject.As<O10Transaction>().Content.Should().Be(content);
        }

        [Fact]
        public async Task AddTransactionAndUpdateRegistryHeightTest()
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
                .And.Match<O10Transaction>(t => t.HashKey != null && t.HashKey.Hash.Equals32(hash))
                .And.Subject.As<O10Transaction>().Content.Should().Be(content);

            await dataAccessService.UpdateRegistryInfo(transaction.O10TransactionId, 17, CancellationToken.None);
            
            var lastTransaction = await dataAccessService.GetLastTransactionalBlock(sourceKey, CancellationToken.None);

            lastTransaction
                .Should()
                .NotBeNull()
                .And.Match<O10Transaction>(t => t.O10TransactionId == transaction.O10TransactionId)
                .And.Match<O10Transaction>(t => t.RegistryHeight == 17);
        }

        [Fact]
        public async Task GetLastTransactionalBlock()
        {
            var dataAccessService = _coreFixture.ServiceProvider.GetRequiredService<IDataAccessService>() as DataAccessService;

            string[] contents = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            byte[][] hash = { CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed() };
            byte[] source = CryptoHelper.GetRandomSeed();
            IKey sourceKey = _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(source);
            O10Transaction[] transactions = new O10Transaction[3];

            transactions[0] = await dataAccessService.AddTransaction(sourceKey, 1, 1, contents[0], hash[0], CancellationToken.None);
            transactions[1] = await dataAccessService.AddTransaction(sourceKey, 1, 2, contents[1], hash[1], CancellationToken.None);
            transactions[2] = await dataAccessService.AddTransaction(sourceKey, 1, 3, contents[2], hash[2], CancellationToken.None);

            var lastTransaction = await dataAccessService.GetLastTransactionalBlock(sourceKey, CancellationToken.None);

            lastTransaction
                .Should()
                .NotBeNull()
                .And.BeEquivalentTo(transactions[2], c => c.Excluding(m => m.HashKey));
        }

        [Fact]
        public async Task GetTransactionTest()
        {
            var dataAccessService = _coreFixture.ServiceProvider.GetRequiredService<IDataAccessService>() as DataAccessService;

            string[] contents = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            byte[][] hash = { CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed() };
            byte[] source = CryptoHelper.GetRandomSeed();
            IKey sourceKey = _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(source);
            O10Transaction[] transactions = new O10Transaction[3];

            transactions[0] = await dataAccessService.AddTransaction(sourceKey, 1, 1, contents[0], hash[0], CancellationToken.None);
            transactions[1] = await dataAccessService.AddTransaction(sourceKey, 1, 2, contents[1], hash[1], CancellationToken.None);
            transactions[2] = await dataAccessService.AddTransaction(sourceKey, 1, 3, contents[2], hash[2], CancellationToken.None);

            await dataAccessService.UpdateRegistryInfo(transactions[0].O10TransactionId, 17, CancellationToken.None);
            await dataAccessService.UpdateRegistryInfo(transactions[1].O10TransactionId, 17, CancellationToken.None);
            await dataAccessService.UpdateRegistryInfo(transactions[2].O10TransactionId, 18, CancellationToken.None);

            var transaction1 = await dataAccessService.GetTransaction(_coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(hash[1]), 17, CancellationToken.None);
            var transaction2 = await dataAccessService.GetTransaction(_coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(hash[2]), CancellationToken.None);

            transaction1
                .Should()
                .NotBeNull()
                .And.BeEquivalentTo(transactions[1], c => c.Excluding(t => t.HashKey).Excluding(t => t.Source).Excluding(t => t.RegistryHeight));

            transaction2
                .Should()
                .NotBeNull()
                .And.BeEquivalentTo(transactions[2], c => c.Excluding(t => t.HashKey).Excluding(t => t.Source).Excluding(t => t.RegistryHeight));
        }
    }
}
