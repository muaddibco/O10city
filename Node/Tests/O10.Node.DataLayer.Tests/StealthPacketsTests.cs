using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.Identity;
using O10.Core.ExtensionMethods;
using O10.Core.Persistency;
using O10.Crypto.ConfidentialAssets;
using O10.Node.DataLayer.DataAccess;
using O10.Node.DataLayer.Specific.Stealth;
using O10.Node.DataLayer.Specific.Stealth.DataContexts.SqlServer;
using O10.Node.DataLayer.Specific.Stealth.Model;
using O10.Tests.Core.Fixtures;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace O10.Node.DataLayer.Tests
{
    public class StealthPacketsTests : DalTestBase
    {
        private readonly DataContext _dataContext;

        public StealthPacketsTests(SqlServerDockerCollectionFixture fixture, CoreFixture coreFixture) : base(fixture, coreFixture) => _dataContext = new DataContext();

        protected override NodeDataContextBase GetNodeDataContext() => _dataContext;

        protected override void RegisterServices()
        {
            _coreFixture.ServiceCollection.AddSingleton<IDataAccessService, DataAccessService>();
        }

        [Fact]
        public async Task AddTransactionTest()
        {
            var dataAccessService = _coreFixture.ServiceProvider.GetRequiredService<IDataAccessService>() as DataAccessService;

            byte[] keyImage = CryptoHelper.GetRandomSeed();
            IKey keyImageKey = _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(keyImage);

            byte[] destinationKey = CryptoHelper.GetRandomSeed();
            IKey destinationKeyKey = _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(destinationKey);

            string content = Guid.NewGuid().ToString();

            var hash = CryptoHelper.GetRandomSeed();

            var transaction = await dataAccessService.AddTransaction(keyImageKey, 1, destinationKeyKey, content, hash, CancellationToken.None);
        
            transaction
                .Should()
                .NotBeNull()
                .And.Match<StealthTransaction>(t => t.KeyImage != null && t.KeyImage.Value != null && t.KeyImage.Value.Equals32(keyImage))
                .And.Match<StealthTransaction>(t => t.HashKey != null && t.HashKey.Hash != null && t.HashKey.Hash.Equals32(hash))
                .And.Match<StealthTransaction>(t => t.DestinationKey != null && t.DestinationKey.Equals32(destinationKey))
                .And.Match<StealthTransaction>(t => t.BlockType == 1)
                .And.Subject.As<StealthTransaction>().Content.Should().Be(content);

            var hashExpected = await dataAccessService.GetHashByKeyImage(keyImage, CancellationToken.None);

            hashExpected
                .Should()
                .BeEquivalentTo(hash);
        }

        [Fact]
        public async Task AddTransactionAndUpdateRegistryHeightTest()
        {
            var dataAccessService = _coreFixture.ServiceProvider.GetRequiredService<IDataAccessService>() as DataAccessService;

            byte[] keyImage = CryptoHelper.GetRandomSeed();
            IKey keyImageKey = _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(keyImage);

            byte[] destinationKey = CryptoHelper.GetRandomSeed();
            IKey destinationKeyKey = _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(destinationKey);

            string content = Guid.NewGuid().ToString();

            var hash = CryptoHelper.GetRandomSeed();

            var transaction = await dataAccessService.AddTransaction(keyImageKey, 1, destinationKeyKey, content, hash, CancellationToken.None);

            transaction
                .Should()
                .NotBeNull()
                .And.Match<StealthTransaction>(t => t.KeyImage != null && t.KeyImage.Value != null && t.KeyImage.Value.Equals32(keyImage))
                .And.Match<StealthTransaction>(t => t.HashKey != null && t.HashKey.Hash != null && t.HashKey.Hash.Equals32(hash))
                .And.Match<StealthTransaction>(t => t.DestinationKey != null && t.DestinationKey.Equals32(destinationKey))
                .And.Match<StealthTransaction>(t => t.BlockType == 1)
                .And.Subject.As<StealthTransaction>().Content.Should().Be(content);
        
            await dataAccessService.UpdateRegistryInfo(transaction.StealthTransactionId, 17, CancellationToken.None);

            var lastTransaction = await dataAccessService.GetTransaction(_coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(hash), CancellationToken.None);

            lastTransaction
                .Should()
                .NotBeNull()
                .And.Match<StealthTransaction>(t => t.StealthTransactionId == transaction.StealthTransactionId)
                .And.Match<StealthTransaction>(t => t.RegistryHeight == 17);
        }

        [Fact]
        public async Task GetTransactionTest()
        {
            var dataAccessService = _coreFixture.ServiceProvider.GetRequiredService<IDataAccessService>() as DataAccessService;

            byte[][] keyImages = new[] { CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed() };
            IKey[] keyImageKeys = new[] {
                _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(keyImages[0]),
                _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(keyImages[1]),
                _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(keyImages[2])
            };

            byte[][] destinationKeys = new[] { CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed() };
            IKey[] destinationKeyKeys = new[] {
                _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(destinationKeys[0]),
                _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(destinationKeys[1]),
                _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(destinationKeys[2])
            };

            string[] contents = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            var hashes = new[] { CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed(), CryptoHelper.GetRandomSeed() };

            StealthTransaction[] transactions = new StealthTransaction[3];

            transactions[0] = await dataAccessService.AddTransaction(keyImageKeys[0], 1, destinationKeyKeys[0], contents[0], hashes[0], CancellationToken.None);
            transactions[1] = await dataAccessService.AddTransaction(keyImageKeys[1], 1, destinationKeyKeys[1], contents[1], hashes[1], CancellationToken.None);
            transactions[2] = await dataAccessService.AddTransaction(keyImageKeys[2], 1, destinationKeyKeys[2], contents[2], hashes[2], CancellationToken.None);

            await dataAccessService.UpdateRegistryInfo(transactions[0].StealthTransactionId, 17, CancellationToken.None);
            await dataAccessService.UpdateRegistryInfo(transactions[1].StealthTransactionId, 17, CancellationToken.None);
            await dataAccessService.UpdateRegistryInfo(transactions[2].StealthTransactionId, 18, CancellationToken.None);

            var transaction1 = await dataAccessService.GetTransaction(17, _coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(hashes[1]), CancellationToken.None);
            var transaction2 = await dataAccessService.GetTransaction(_coreFixture.ServiceProvider.GetRequiredService<IIdentityKeyProvidersRegistry>().GetInstance().GetKey(hashes[2]), CancellationToken.None);

            transaction1
                .Should()
                .NotBeNull()
                .And.BeEquivalentTo(transactions[1], c => c.Excluding(t => t.HashKey).Excluding(t => t.KeyImage).Excluding(t => t.RegistryHeight));

            transaction2
                .Should()
                .NotBeNull()
                .And.BeEquivalentTo(transactions[2], c => c.Excluding(t => t.HashKey).Excluding(t => t.KeyImage).Excluding(t => t.RegistryHeight));
        }
    }
}
