using NSubstitute;
using O10.Client.Common.Communication;
using O10.Core.Configuration;
using O10.Core.Identity;
using O10.Tests.Core.Fixtures;
using O10.Client.DataLayer.Services;
using O10.Tests.Core;
using Xunit.Abstractions;
using O10.Client.Common.Interfaces;

namespace O10.Client.Common.Tests
{

    public class StateWalletSynchronizerTest: TestBase
    {
        #region ============================================ MEMBERS ==================================================

        private readonly StateWalletSynchronizer _stateWalletSynchronizer;

        private readonly IStateClientCryptoService _clientCryptoService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
        private readonly IConfigurationService _configurationService;

        #endregion

        #region ========================================== CONSTRUCTORS ===============================================

        public StateWalletSynchronizerTest(CoreFixture coreFixture, ITestOutputHelper testOutputHelper)
            : base(coreFixture, testOutputHelper)
        {
            _identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
            _configurationService = Substitute.For<IConfigurationService>();

            _stateWalletSynchronizer = new StateWalletSynchronizer(
                _dataAccessService = Substitute.For<IDataAccessService>(),
                _clientCryptoService = Substitute.For<IStateClientCryptoService>(),
                CoreFixture.LoggerService);
        }

        #endregion

        #region ======================================== PUBLIC FUNCTIONS =============================================

        #endregion

        #region ======================================== PRIVATE FUNCTIONS ============================================

        #endregion

    }
}