using NSubstitute;
using O10.Client.Common.Communication;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.Configuration;
using O10.Core.Identity;
using O10.Tests.Core;
using O10.Tests.Core.Fixtures;
using Xunit.Abstractions;

namespace O10.Client.Common.Tests
{
    public class UtxoWalletSynchronizerTest : TestBase
    {
        #region ============================================ MEMBERS ==================================================

        private readonly StealthWalletSynchronizer _utxoWalletSynchronizer;

        private readonly IDataAccessService _dataAccessService;
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly IConfigurationService _configurationService;
		private readonly IAssetsService _assetsService;
        private readonly IStealthClientCryptoService _clientCryptoService;
        private readonly IStealthTransactionsService _transactionsService;

        #endregion

        #region ========================================== CONSTRUCTORS ===============================================

        public UtxoWalletSynchronizerTest(CoreFixture coreFixture, ITestOutputHelper testOutputHelper)
            : base(coreFixture, testOutputHelper)
        {
			_utxoWalletSynchronizer = new StealthWalletSynchronizer(
				_dataAccessService = Substitute.For<IDataAccessService>(),
                _clientCryptoService = Substitute.For<IStealthClientCryptoService>(),
                _transactionsService = Substitute.For<IStealthTransactionsService>(),
                _assetsService = Substitute.For<IAssetsService>(),
                CoreFixture.LoggerService);
        }

        #endregion

        #region ======================================== PUBLIC FUNCTIONS =============================================


        #endregion

        #region ======================================== PRIVATE FUNCTIONS ============================================

        #endregion

    }
}
