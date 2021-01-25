using NSubstitute;
using System;
using System.Collections.Generic;
using O10.Transactions.Core.DataModel;
using O10.Client.Common.Interfaces;
using O10.Client.DataModel.Enums;
using O10.Client.DataModel.Model;
using O10.Client.DataModel.Services;
using O10.Network.Tests.Fixtures;
using O10.Client.Web.Portal.Services;
using Xunit;

namespace O10.Client.Web.Portal.Test
{
    public class AccountsServiceTest : IClassFixture<DependencyInjectionFixture>
    {
        #region ============================================ MEMBERS ==================================================

        private readonly AccountsService _accountService;

        private readonly IDataAccessService _externalDataAccessService;
        private readonly IExecutionContextManager _executionContextManager;
        private readonly IIdentityAttributesService _identityAttributesService;
        private readonly IGatewayService _gatewayService;

        #endregion

        #region ========================================== CONSTRUCTORS ===============================================

        public AccountsServiceTest()
        {
            _accountService = new AccountsService(
                _externalDataAccessService = Substitute.For<IDataAccessService>(),
                _executionContextManager = Substitute.For<IExecutionContextManager>(),
                _identityAttributesService = Substitute.For<IIdentityAttributesService>(),
                _gatewayService = Substitute.For<IGatewayService>());

            _gatewayService.GetLastRegistryCombinedBlock().ReturnsForAnyArgs(new RegistryCombinedBlockModel(1, null, null));
        }

        #endregion

        #region ======================================== PUBLIC FUNCTIONS =============================================

        [Fact]
        public void AuthenticateSucceeded()
        {
            var discriptor = 
                _accountService.Authenticate(new ulong { }, string.Empty);

			_externalDataAccessService
				.WhenForAnyArgs(t => 
                    t.GetAccount(null)).Received(1);

            Assert.NotNull(discriptor);
        }

        [Fact]
        public void CleanSucceeded()
        {
             _accountService
                .Clean(new ulong { });

            _executionContextManager
                .WhenForAnyArgs(t => 
                    t.Clean(0)).Received(1);

            Assert.True(true);
        }

        [Fact]
        public void CreateUserSucceeded()
        {
            try
            {
                _accountService
                    .Create(AccountType.User, string.Empty, string.Empty);
            }
            catch (Exception)
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void CreateIdProviderSucceeded()
        {
            try
            {
                _accountService
                    .Create(AccountType.IdentityProvider, string.Empty, string.Empty);
            }
            catch (Exception)
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void CreateServiceProviderSucceeded()
        {
            try
            {
                _accountService
                    .Create(AccountType.ServiceProvider, string.Empty, string.Empty);
            }
            catch (Exception)
            {
                Assert.True(false);
            }
        }

        [Fact]
        public void GetAllNotNull()
        {
			_externalDataAccessService
				.WhenForAnyArgs(t =>
                    t.GetAccounts())
                        .Received(1);

            var accounts = 
                _accountService
                    .GetAll();

            Assert.NotNull(accounts);
        }

        [Fact]
        public void GetAllReturnsDataTwo()
        {
			_externalDataAccessService.GetAccounts()
                .ReturnsForAnyArgs(
                    new List<Account>
                    {
                        new Account
                        {
                            AccountId = new ulong{ },
                            AccountInfo = "Hellloooooouu, la la la"
                        },
                        new Account
                        {
                            AccountId = new ulong{ },
                            AccountInfo = "Hellloooooo, Gerry!!"
                        },
                    });
               

            var accounts =
                _accountService
                    .GetAll();

            Assert.True(accounts.Count == 2);
        }


        [Fact]
        public void GetAllReturnsDataTwoArgsNotNull()
        {
			_externalDataAccessService.GetAccounts()
                .ReturnsForAnyArgs(
                    new List<Account>
                    {
                        new Account
                        {
                            AccountId = new ulong{ },
                            AccountInfo = "Hellloooooouu, la la la"
                        },
                        new Account
                        {
                            AccountId = new ulong{ },
                            AccountInfo = "Hellloooooo, Gerry!!"
                        },
                    });


            var accounts =
                _accountService
                    .GetAll();

            Assert.NotNull(accounts);
        }

        [Fact]
        public void GetAllReturnsDataOne()
        {
			_externalDataAccessService.GetAccounts()
                .ReturnsForAnyArgs(
                    new List<Account>
                    {
                        new Account
                        {
                            AccountId = new ulong{ },
                            AccountInfo = "Hellloooooouu, la la la"
                        }
                    });

            var accounts =
                _accountService
                    .GetAll();

            Assert.True(accounts.Count == 1);
        }

        [Fact]
        public void GetAllReturnsNull()
        {
			_externalDataAccessService.GetAccounts()
                .ReturnsForAnyArgs(t => null);

            var accounts =
                _accountService
                    .GetAll();

            Assert.Null(accounts);
        }

        [Fact]
        public void GetByIdReturnsAccount()
        {
			_externalDataAccessService.GetAccount(1)
                .Returns(new Account
                {
                    AccountId = 1,
                    AccountInfo = "Hiiii"
                });

            var accounts =
                _accountService
                    .GetById(1);

            Assert.True(accounts.AccountId == 1);
        }

        [Fact]
        public void GetByIdReturnsAccountIsNotNull()
        {
			_externalDataAccessService.GetAccount(1)
                .Returns(new Account
                {
                    AccountId = 1,
                    AccountInfo = "Hiiii"
                });

            var accounts =
                _accountService
                    .GetById(1);

            Assert.NotNull(accounts);
        }

        [Fact]
        public void GetByIdReturnsNull()
        {
			_externalDataAccessService.GetAccount(1)
                .Returns(t => null);

            var account =
                _accountService
                    .GetById(1);

            Assert.Null(account);
        }

        [Fact]
        public void GetByPublicKeyReturnsAccount()
        {
			_externalDataAccessService.GetAccount(new byte[] { 1 })
                .Returns(new Account
                {
                    AccountId = 1,
                    AccountInfo = "Hiiii"
                });

            var accounts =
                _accountService
                    .GetByPublicKey(new byte[] { 1 });

            Assert.True(accounts.AccountId == 1);
        }

        [Fact]
        public void GetByPublicKeyReturnsAccountIsNotNull()
        {
			_externalDataAccessService.GetAccount(new byte[] { 1 })
                .Returns(new Account
                {
                    AccountId = 1,
                    AccountInfo = "Hiiii"
                });

            var accounts =
                _accountService
                    .GetByPublicKey(new byte[] { 1 });

            Assert.NotNull(accounts);
        }

        [Fact]
        public void GetByPublicKeyReturnsNull()
        {
			_externalDataAccessService.GetAccount(new byte[] { 1 })
                .Returns(t => null);

            var account =
                _accountService
                    .GetByPublicKey(new byte[] { 1 });

            Assert.Null(account);
        }

        [Fact]
        public void GetByPublicKeyWrongKeyReturnsNothing3()
        {
			_externalDataAccessService.GetAccount(new byte[] { 1 })
                .Returns(new Account
                {
                    AccountId = 1,
                    AccountInfo = "Hiiii"
                });

            var account =
                _accountService
                    .GetByPublicKey(new byte[] { 2 });

            Assert.Null(account);
        }

        [Fact]
        public void GetByPublicKeyWrongKeyReturnsNothing()
        {
			_externalDataAccessService.GetAccount(new byte[] { 1 })
                .Returns(new Account
                {
                    AccountId = 1,
                    AccountInfo = "Hiiii"
                });

            var account =
                _accountService
                    .GetByPublicKey(new byte[] { 2 });

            Assert.Null(account);
        }

        [Fact]
        public void GetByPublicKeyWrongKeyReturnsNothing2()
        {
			_externalDataAccessService.GetAccount(new byte[] { 1 })
                .Returns(t => null);

            var account =
                _accountService
                    .GetByPublicKey(new byte[] { 2 });

            Assert.Null(account);
        }

        [Fact]
        public void UpdateReturnsNullInCaseUser()
        {
            Assert.Throws<NotImplementedException>(() => 
                _accountService.Update(
                        new Client.Common.Entities.AccountDescriptor
                        {
                            AccountId = 1, 
                            AccountInfo = "",
                            AccountType = AccountType.User,
                            PublicSpendKey = null,
                            PublicViewKey = null,
                            SecretSpendKey = null,
                            SecretViewKey = null
                        }));
        }

        [Fact]
        public void UpdateReturnsNullInCaseServiceProvider()
        {
            Assert.Throws<NotImplementedException>(() =>
                _accountService.Update(
                        new Client.Common.Entities.AccountDescriptor
                        {
                            AccountId = 1,
                            AccountInfo = "",
                            AccountType = AccountType.ServiceProvider,
                            PublicSpendKey = null,
                            PublicViewKey = null,
                            SecretSpendKey = null,
                            SecretViewKey = null
                        }));
        }

        [Fact]
        public void UpdateReturnsNullInCaseIdentityProvider()
        {
            Assert.Throws<NotImplementedException>(() =>
                _accountService.Update(
                        new Client.Common.Entities.AccountDescriptor
                        {
                            AccountId = 1,
                            AccountInfo = "",
                            AccountType = AccountType.IdentityProvider,
                            PublicSpendKey = null,
                            PublicViewKey = null,
                            SecretSpendKey = null,
                            SecretViewKey = null
                        }));
        }

        [Fact]
        public void UpdateReturnsNullInCaseAccountOne()
        {
            Assert.Throws<NotImplementedException>(() =>
                _accountService.Update(
                        new Client.Common.Entities.AccountDescriptor
                        {
                            AccountId = 1,
                            AccountInfo = "",
                            AccountType = AccountType.User,
                            PublicSpendKey = null,
                            PublicViewKey = null,
                            SecretSpendKey = null,
                            SecretViewKey = null
                        }));
        }

        [Fact]
        public void UpdateReturnsNullInCaseAccountTwo()
        {
            Assert.Throws<NotImplementedException>(() =>
                _accountService.Update(
                        new Client.Common.Entities.AccountDescriptor
                        {
                            AccountId = 2,
                            AccountInfo = "",
                            AccountType = AccountType.User,
                            PublicSpendKey = null,
                            PublicViewKey = null,
                            SecretSpendKey = null,
                            SecretViewKey = null
                        }));
        }

        [Fact]
        public void UpdateReturnsNullInCaseAccountThree()
        {
            Assert.Throws<NotImplementedException>(() =>
                _accountService.Update(
                        new Client.Common.Entities.AccountDescriptor
                        {
                            AccountId = 3,
                            AccountInfo = "",
                            AccountType = AccountType.User,
                            PublicSpendKey = null,
                            PublicViewKey = null,
                            SecretSpendKey = null,
                            SecretViewKey = null
                        }));
        }

        [Fact]
        public void UpdateReturnsNullInCaseAccount5()
        {
            Assert.Throws<NotImplementedException>(() =>
                _accountService.Update(
                        new Client.Common.Entities.AccountDescriptor
                        {
                            AccountId = 5,
                            AccountInfo = "",
                            AccountType = AccountType.User,
                            PublicSpendKey = null,
                            PublicViewKey = null,
                            SecretSpendKey = null,
                            SecretViewKey = null
                        }));
        }

        [Fact]
        public void UpdateReturnsNullInCaseAccount6()
        {
            Assert.Throws<NotImplementedException>(() =>
                _accountService.Update(
                        new Client.Common.Entities.AccountDescriptor
                        {
                            AccountId = 6,
                            AccountInfo = "",
                            AccountType = AccountType.User,
                            PublicSpendKey = null,
                            PublicViewKey = null,
                            SecretSpendKey = null,
                            SecretViewKey = null
                        }));
        }

        public void UpdateReturnsNullInCaseAccount7()
        {
            Assert.Throws<NotImplementedException>(() =>
                _accountService.Update(
                        new Client.Common.Entities.AccountDescriptor
                        {
                            AccountId = 7,
                            AccountInfo = "",
                            AccountType = AccountType.User,
                            PublicSpendKey = null,
                            PublicViewKey = null,
                            SecretSpendKey = null,
                            SecretViewKey = null
                        }));
        }

        [Fact]
        public void DuplicateAccountReturnsAccount()
        {
			_externalDataAccessService.DuplicateUserAccount(
                1,
                "this is account")
                    .Returns((ulong)1);

            var account =
                _accountService
                    .DuplicateAccount(1, "this is account");

            Assert.True(account == 1);
        }

        [Fact]
        public void DuplicateAccountReturnsAccountNew()
        {
			_externalDataAccessService.DuplicateUserAccount(
                2,
                "this is account")
                    .Returns((ulong)1);

            var account =
                _accountService
                    .DuplicateAccount(2, "this is account");

            Assert.True(account == 2);
        }

        [Fact]
        public void DuplicateAccountFails()
        {
			_externalDataAccessService.DuplicateUserAccount(
                2,
                "this is account")
                    .Returns(null);

            var account =
                _accountService
                    .DuplicateAccount(2, "this is account");

            Assert.Null(account);
        }

        #endregion

        #region ======================================== PRIVATE FUNCTIONS ============================================

        #endregion

    }
}