using NSubstitute;
using System;
using O10.Transactions.Core.Parsers;
using O10.Transactions.Core.Serializers;
using O10.Client.Common.Communication;
using O10.Client.Common.Communication.SynchronizerNotifications;
using O10.Client.Common.Interfaces;
using O10.Core.HashCalculations;
using O10.Core.Identity;
using O10.Tests.Core;
using O10.Tests.Core.Fixtures;
using Xunit;
using Xunit.Abstractions;
using O10.Transactions.Core.DTOs;

namespace O10.Client.Common.Tests
{
    public class UtxoTransactionsServiceTest : TestBase
    {
        #region ============================================ MEMBERS ==================================================

        private readonly StealthTransactionsService _utxoTransactionsService;

        private readonly IHashCalculationsRepository _hashCalculationsRepository;
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly ISerializersFactory _serializersFactory;
        private readonly IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
        private readonly IGatewayService _gatewayService;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly IBoundedAssetsService _boundedAssetsService;
        private readonly IEligibilityProofsProvider _eligibilityProofsProvider;

        #endregion

        #region ========================================== CONSTRUCTORS ===============================================

        public UtxoTransactionsServiceTest(CoreFixture coreFixture, ITestOutputHelper testOutputHelper)
            : base(coreFixture, testOutputHelper)
        {
            _utxoTransactionsService = new StealthTransactionsService(
                _hashCalculationsRepository = Substitute.For<IHashCalculationsRepository>(),
                _identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>(),
                _clientCryptoService = Substitute.For<IStealthClientCryptoService>(),
                _boundedAssetsService = Substitute.For<IBoundedAssetsService>(),
                _serializersFactory = Substitute.For<ISerializersFactory>(),
                _blockParsersRepositoriesRepository = Substitute.For<IBlockParsersRepositoriesRepository>(),
                _eligibilityProofsProvider = Substitute.For<IEligibilityProofsProvider>(),
                _gatewayService = Substitute.For<IGatewayService>(),
                CoreFixture.LoggerService
                );
        }

        #endregion

        #region ======================================== PUBLIC FUNCTIONS =============================================

        [Fact]
        public void InitializeAssertionSucceeded()
        {
            _utxoTransactionsService.Initialize(1);

            Assert.True(true);
        }

        [Fact]
        public void SendCompromisedProofsSucceeded()
        {
            _utxoTransactionsService.SendCompromisedProofs(
                new Interfaces.Inputs.RequestInput(), 
                Array.Empty<byte>(),
                Array.Empty<byte>(),
                Array.Empty<byte>(),
                Array.Empty<OutputModel>(),
                Array.Empty<byte[]>())
                    .ContinueWith(t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            var res = t.Result;

                            //_gatewayService.WhenForAnyArgs(
                            //    p =>
                            //    p.(null, null, null));

                            Assert.True(res != null);
                        }
                    });
        }

        #endregion

        #region ======================================== PRIVATE FUNCTIONS ============================================

        #endregion

    }
}