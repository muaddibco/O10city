using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using O10.Client.Common.Interfaces;
using O10.Client.DataModel.Services;
using O10.Network.Tests.Fixtures;
using O10.Client.Web.Portal.Controllers;
using O10.Client.Web.Portal.Dtos;
using O10.Client.Web.Portal.Helpers;
using O10.Client.Web.Portal.Services;
using Xunit;

namespace O10.Client.Web.Portal.Test
{
    public class ServiceProvidersControllerTest : IClassFixture<DependencyInjectionFixture>
    {
        #region ============================================ MEMBERS ==================================================

        private ServiceProvidersController _serviceProvidersController;

        private readonly IAccountsService _accountsService;
        private readonly IDataAccessService _externalDataAccessService;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IOptions<AppSettings> _appSettings;



        #endregion

        #region ========================================== CONSTRUCTORS ===============================================

        public ServiceProvidersControllerTest()
        {
            _identityAttributesService = Substitute.For<IIdentityAttributesService>();
            _externalDataAccessService = Substitute.For<IDataAccessService>();
            _accountsService = Substitute.For<IAccountsService>();
            _appSettings = Substitute.For<IOptions<AppSettings>>();

            _serviceProvidersController = new ServiceProvidersController(_accountsService, _externalDataAccessService, _identityAttributesService, _appSettings);
        }

        #endregion

        #region ======================================== PUBLIC FUNCTIONS =============================================

        [Fact]
        public void GetAllSucceeded()
        {
            IActionResult res = 
                _serviceProvidersController
                .GetAll
                ();

            Assert
                .IsType
                <OkObjectResult>
                (res);
        }

        [Fact]
        public void GetRegistrationsSucceeded()
        {
            IActionResult res = 
                _serviceProvidersController
                .GetRegistrations
                ();

            Assert
                .IsType
                <OkObjectResult>
                (res);
        }

        [Fact]
        public void GetByIdSucceeded()
        {
            IActionResult res =
                _serviceProvidersController
                .GetById(new ulong{ });

            Assert
                .IsType
                <OkObjectResult>
                (res);
        }

        [Fact]
        public void GetIdentityAttributeValidationDescriptorsSucceeded()
        {
            IActionResult res =
                _serviceProvidersController
                .GetIdentityAttributeValidationDescriptors();

            Assert
                .IsType
                <OkObjectResult>
                (res);
        }

        [Fact]
        public void GetIdentityAttributeValidationsSucceeded()
        {
            IActionResult res =
                _serviceProvidersController
                .GetIdentityAttributeValidations();

            Assert
                .IsType
                <OkObjectResult>
                (res);
        }

        [Fact]
        public void UpdateIdentityAttributeValidationDefinitionsSucceeded()
        {
            IActionResult res =
                _serviceProvidersController
                .UpdateIdentityAttributeValidationDefinitions(new IdentityAttributeValidationDefinitionsDto { });

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