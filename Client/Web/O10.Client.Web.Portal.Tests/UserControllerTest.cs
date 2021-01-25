using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using O10.Client.Common.Interfaces;
using O10.Client.DataModel.Services;
using O10.Core.Configuration;
using O10.Network.Tests.Fixtures;
using O10.Client.Web.Portal.Configuration;
using O10.Client.Web.Portal.Controllers;
using O10.Client.Web.Portal.Dtos;
using O10.Client.Web.Portal.Services;
using Xunit;

namespace O10.Client.Web.Portal.Test
{
    public class UserControllerTest : IClassFixture<DependencyInjectionFixture>
    {
        #region ============================================ MEMBERS ==================================================

        private UserController _userController;

        private readonly IAccountsService _accountsService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly IAssetsService _assetsService;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IFacesService _facesService;
        private readonly IConfigurationService _configurationService;
        private readonly IPortalConfiguration _portalConfiguration;
        private readonly IGatewayService _gatewayService;

        #endregion

        #region ========================================== CONSTRUCTORS ===============================================

        public UserControllerTest()
        {
            _accountsService = Substitute.For<IAccountsService>();
            _executionContextManager = Substitute.For<IExecutionContextManager>();
            _assetsService = Substitute.For<IAssetsService>();
            _identityAttributesService = Substitute.For<IIdentityAttributesService>();
            _dataAccessService = Substitute.For<IDataAccessService>();
            _facesService = Substitute.For<IFacesService>();
            _configurationService = Substitute.For<IConfigurationService>();
            _portalConfiguration = Substitute.For<IPortalConfiguration>();
            _gatewayService = Substitute.For<IGatewayService>();

            _userController = new UserController(_accountsService, _executionContextManager, _assetsService, _identityAttributesService, _dataAccessService, _gatewayService, _configurationService);
        }

        #endregion

        #region ======================================== PUBLIC FUNCTIONS =============================================

        [Fact]
        public void ValidateAttributeSucceeded()
        {
            AccountDto acdto = 
                new AccountDto();
            IActionResult res = 
                _userController
                .ValidateAttribute(
                    new UserAttributeDto { });

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void GetUserAttributesSucceeded()
        {
            AccountDto acdto = 
                new AccountDto();
            IActionResult res = 
                _userController
                .GetUserAttributes();

            Assert
                .IsType<OkObjectResult>
                (res);
        }


        [Fact]
        public void SendCompromisedProofsSucceeded()
        {
            AccountDto acdto = 
                new AccountDto();
            IActionResult res = 
                _userController
                .SendCompromisedProofs(
                    new UnauthorizedUseDto { });

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void SendOnboardingWithValidationsRequestSucceeded()
        {
            AccountDto acdto =
                new AccountDto();
            IActionResult res = 
                _userController
                .SendOnboardingWithValidationsRequest(
                    new UserAttributeTransferWithValidationsDto { });

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void GetUserRegistrationsSucceeded()
        {
            AccountDto acdto 
                = new AccountDto();
            IActionResult res 
                = _userController
                .GetUserRegistrations();

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void GetUserAssociatedAttributesSucceeded()
        {
            AccountDto acdto 
                = new AccountDto();
            IActionResult res 
                = _userController
                .GetUserAssociatedAttributes();

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void UpdateUserAssociatedAttributesSucceeded()
        {
            AccountDto acdto 
                = new AccountDto();
            IActionResult res 
                = _userController
                .UpdateUserAssociatedAttributes(
                    Array.Empty<UserAssociatedAttributeDto>());

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void SendAuthenticationRequestSucceeded()
        {
            AccountDto acdto = 
                new AccountDto();
            IActionResult res = 
                _userController.SendAuthenticationRequest(
                    new UserAttributeTransferDto { });

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void SendOnboardingRequestSucceeded()
        {
            AccountDto acdto =
                new AccountDto();
            IActionResult res = 
                _userController
                .SendOnboardingRequest(
                    new UserAttributeTransferDto { });

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        [Fact]
        public void GetActionTypeSucceeded()
        {
            AccountDto acdto =
                new AccountDto();
            IActionResult res =
                _userController
                .GetActionType(string.Empty);

            Assert
                .IsNotType<BadRequestObjectResult>
                (res);
        }

        [Fact]
        public void GetSpValidationsSucceeded()
        {
            AccountDto acdto =
                new AccountDto();
            IActionResult res =
                _userController
                .GetSpValidations(string.Empty);

            Assert
                .IsType<OkObjectResult>
                (res);
        }

        #endregion

        #region ======================================== PRIVATE FUNCTIONS ============================================

        #endregion

    }
}