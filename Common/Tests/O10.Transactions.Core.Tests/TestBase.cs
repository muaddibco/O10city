using Chaos.NaCl;
using System;
using O10.Transactions.Core.Enums;
using O10.Core.Identity;
using NSubstitute;
using O10.Core.HashCalculations;
using O10.Core.Cryptography;
using O10.Transactions.Core.Parsers;
using O10.Crypto.HashCalculations;
using O10.Crypto.ConfidentialAssets;
using O10.Crypto;
using O10.Tests.Core.Fixtures;
using Xunit.Abstractions;
using O10.Tests.Core;
using O10.Crypto.Models;
using O10.Transactions.Core.Ledgers;

namespace O10.Transactions.Core.Tests
{
    public abstract class BlockchainCoreTestBase : TestBase
    {
        protected IIdentityKeyProvider _transactionHashKeyProvider;
        protected IIdentityKeyProvider _identityKeyProvider;
        protected IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        protected IHashCalculationsRepository _hashCalculationRepository;
        protected IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
        protected IBlockParsersRepository _blockParsersRepository;
        protected ISigningService _signingService;
        protected ISigningService _utxoSigningService;
        protected byte[] _privateKey;
        protected byte[] _privateViewKey;
        protected byte[] _publicKey;
        protected byte[] _expandedPrivateKey;

        public BlockchainCoreTestBase(CoreFixture coreFixture, ITestOutputHelper testOutputHelper)
            : base(coreFixture, testOutputHelper)
        {
            _transactionHashKeyProvider = Substitute.For<IIdentityKeyProvider>();
            _identityKeyProvider = Substitute.For<IIdentityKeyProvider>();
            _identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            _hashCalculationRepository = Substitute.For<IHashCalculationsRepository>();
            _blockParsersRepositoriesRepository = Substitute.For<IBlockParsersRepositoriesRepository>();
            _blockParsersRepository = Substitute.For<IBlockParsersRepository>();
            _signingService = Substitute.For<ISigningService>();

            _identityKeyProvidersRegistry.GetInstance().Returns(_identityKeyProvider);
            _identityKeyProvidersRegistry.GetTransactionsIdenityKeyProvider().Returns(_transactionHashKeyProvider);
            _identityKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key32() { Value = c.Arg<Memory<byte>>() });
            _transactionHashKeyProvider.GetKey(null).ReturnsForAnyArgs(c => new Key16() { Value = c.Arg<Memory<byte>>() });
            _hashCalculationRepository.Create(HashType.MurMur).Returns(new MurMurHashCalculation());
            _blockParsersRepositoriesRepository.GetBlockParsersRepository(LedgerType.Registry).ReturnsForAnyArgs(_blockParsersRepository);

            _privateKey = ConfidentialAssetsHelper.GetRandomSeed();
            _privateViewKey = ConfidentialAssetsHelper.GetRandomSeed();
            Ed25519.KeyPairFromSeed(out _publicKey, out _expandedPrivateKey, _privateKey);

            _signingService.WhenForAnyArgs(s => s.Sign(null, null)).Do(c => 
            {
                ((OrderedPacketBase)c.ArgAt<IPacket>(0)).Source = new Key32(_publicKey);
                ((OrderedPacketBase)c.ArgAt<IPacket>(0)).Signature = Ed25519.Sign(((OrderedPacketBase)c.ArgAt<IPacket>(0)).BodyBytes.ToArray(), _expandedPrivateKey);
            });
            _signingService.PublicKeys.Returns(new IKey[] { new Key32() { Value = _publicKey } });

            _utxoSigningService = new StealthSigningService(_identityKeyProvidersRegistry, CoreFixture.LoggerService);
            _utxoSigningService.Initialize(_privateKey, _privateViewKey);
        }
    }
}
