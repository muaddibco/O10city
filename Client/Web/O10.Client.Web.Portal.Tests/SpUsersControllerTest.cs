using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using O10.Client.Common.Interfaces;
using O10.Client.DataModel.Services;
using O10.Network.Tests.Fixtures;
using O10.Client.Web.Portal.Controllers;
using O10.Client.Web.Portal.Services;
using Xunit;

namespace O10.Client.Web.Portal.Test
{
    public class SpUsersControllerTest : IClassFixture<DependencyInjectionFixture>
    {
        #region ============================================ MEMBERS ==================================================

        private SpUsersController _serviceProvidersController;

        private readonly IAccountsService _accountsService;
		private readonly IDataAccessService _dataAccessService;
		private readonly IIdentityAttributesService _identityAttributesService;

        #endregion

        #region ========================================== CONSTRUCTORS ===============================================

        public SpUsersControllerTest()
        {
            _accountsService = Substitute.For<IAccountsService>();
			_dataAccessService = Substitute.For<IDataAccessService>();
			_identityAttributesService = Substitute.For<IIdentityAttributesService>();

			_serviceProvidersController = new SpUsersController(_accountsService, _dataAccessService, _identityAttributesService);
        }

        #endregion

        #region ======================================== PUBLIC FUNCTIONS =============================================

        [Fact]
        public void GetSessionInfoSucceeded()
        {
            IActionResult res =
                _serviceProvidersController
                .GetSessionInfo(new ulong { });

            Assert
                .IsType
                <OkObjectResult>
                (res);
        }

        #endregion

        #region ======================================== PRIVATE FUNCTIONS ============================================

        #endregion

    }
}