using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using O10.Client.Web.Portal.Controllers;
using O10.Client.Web.Portal.Helpers;
using Xunit;
using O10.Client.Web.DataContracts;
using O10.Client.Common.Interfaces;

namespace O10.Client.Web.Portal.Test
{
    public class AccountsControllerTest
    {
        private readonly AccountsController _accountsController;
        private IAccountsService _accountsService;
        private readonly IOptions<AppSettings> _appSettings;

        public AccountsControllerTest()
        {
            _accountsService = Substitute.For<IAccountsService>();
            _appSettings = Substitute.For<IOptions<AppSettings>>();

            _accountsController = new AccountsController(_accountsService, _appSettings);
        }

        [Fact]
        public void AuthenticateSucceeded()
        {
            AccountDto acdto = new AccountDto();
            IActionResult res = _accountsController.Authenticate(acdto);

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void RegisterSucceeded()
        {
            AccountDto acdto = new AccountDto();
            IActionResult res = _accountsController.Register(acdto);

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void GetAllSucceeded()
        {
            AccountDto acdto = new AccountDto();
            IActionResult res = _accountsController.GetAll();

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void GetByIdSucceeded()
        {
            AccountDto acdto = new AccountDto();
            IActionResult res = _accountsController.GetById(new ulong());

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void LogoutSucceeded()
        {
            AccountDto acdto = new AccountDto();
            IActionResult res = _accountsController.Logout();

            Assert.IsType<OkObjectResult>(res);
        }

        [Fact]
        public void DuplicateUserAccountSucceeded()
        {
            AccountDto acdto = new AccountDto();
            IActionResult res = _accountsController.DuplicateUserAccount(new UserAccountReplicationDto { });

            Assert.IsType<OkObjectResult>(res);
        }
    }
}
