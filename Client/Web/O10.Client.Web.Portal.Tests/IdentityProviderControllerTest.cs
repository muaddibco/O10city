using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using O10.Client.Common.Interfaces;
using O10.Client.DataModel.Services;
using O10.Core.Configuration;
using O10.Network.Tests.Fixtures;
using O10.Client.Web.Portal.Controllers;
using O10.Client.Web.Portal.Dtos;
using O10.Client.Web.Portal.Hubs;
using O10.Client.Web.Portal.Services;
using Xunit;
using O10.Client.Web.DataContracts;

namespace O10.Client.Web.Portal.Test
{
    public class IdentityProviderControllerTest : IClassFixture<DependencyInjectionFixture>
    {
        #region ============================================ MEMBERS ==================================================

        private readonly IdentityProviderController _identityProviderController;

        private readonly IExecutionContextManager _executionContextManager;
        private readonly IAssetsService _assetsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IDataAccessService _externalDataAccessService;
        private readonly IIdentityAttributesService _identityAttributesService;
		private readonly IAccountsService _accountsService;
		private readonly IFacesService _facesService;
        private readonly IConfigurationService _configurationService;
        private readonly IHubContext<IdentitiesHub> _hubContext;

        #endregion

        #region ========================================== CONSTRUCTORS ===============================================

        public IdentityProviderControllerTest()
        {
            _executionContextManager = Substitute.For<IExecutionContextManager>();
            _assetsService = Substitute.For<IAssetsService>();
            _dataAccessService = Substitute.For<IDataAccessService>();
            _externalDataAccessService = Substitute.For<IDataAccessService>();
            _identityAttributesService = Substitute.For<IIdentityAttributesService>();
			_accountsService = Substitute.For<IAccountsService>();
			_facesService = Substitute.For<IFacesService>();
            _configurationService = Substitute.For<IConfigurationService>();
            _hubContext = Substitute.For<IHubContext<IdentitiesHub>>();

            _identityProviderController = new IdentityProviderController(_executionContextManager, _assetsService, _dataAccessService, _externalDataAccessService, _identityAttributesService, _facesService, _accountsService, _configurationService, _hubContext);
        }

        #endregion

        #region ======================================== PUBLIC FUNCTIONS =============================================

        [Fact]
        public void CreateIdentitySucceeded()
        {
            AccountDto acdto = 
                new AccountDto();
            IActionResult res = 
                _identityProviderController
                .CreateIdentity(
                    new IdentityDto { });

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void GetIdentityByIdSucceeded()
        {
            AccountDto acdto = 
                new AccountDto();
            IActionResult res = 
                _identityProviderController
                .GetIdentityById(
                    new ulong { });

            Assert
                .IsType<OkObjectResult>
                (res);
        }
        [Fact]
        public void GetAllIdentitiesSucceeded()
        {
            AccountDto acdto = 
                new AccountDto();
            IActionResult res = 
                _identityProviderController
                .GetAllIdentities(
                    new ulong { });

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void SendAssetIdNewSucceeded()
        {
            AccountDto acdto = 
                new AccountDto();
            IActionResult res = 
                _identityProviderController
                .SendAssetIdNew(
                    new AttributeTransferDetails { });

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void GetAttributesSchemaSucceeded()
        {
            AccountDto acdto = 
                new AccountDto();
            IActionResult res = 
                _identityProviderController
                .GetAttributesSchema();

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        #endregion

        #region ======================================== PRIVATE FUNCTIONS ============================================

        #endregion

    }
}