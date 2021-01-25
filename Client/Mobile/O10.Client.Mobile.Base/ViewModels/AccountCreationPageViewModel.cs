using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Linq;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Enums;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class AccountCreationPageViewModel : ViewModelBase
    {
        private readonly IAccountsService _accountsService;
        private readonly IExecutionContext _executionContext;
        private readonly IPageDialogService _pageDialogService;
        private string _accountInfo;
        private string _password;
        private string _passwordConfirmation;

        public AccountCreationPageViewModel(INavigationService navigationService, IAccountsService accountsService, IExecutionContext executionContext, IPageDialogService pageDialogService) : base(navigationService)
        {
            Title = AppResources.PAGE_TITLE_ACCOUNT_CREATION;
            _accountsService = accountsService;
            _executionContext = executionContext;
            _pageDialogService = pageDialogService;
        }

        public string AccountInfo
        {
            get => _accountInfo;
            set
            {
                SetProperty(ref _accountInfo, value);
            }
        }
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
            }
        }

        public string PasswordConfirmation
        {
            get => _passwordConfirmation;
            set
            {
                SetProperty(ref _passwordConfirmation, value);
            }
        }

        public DelegateCommand ConfirmRegistrationCommand => new DelegateCommand(() =>
        {
            if (!string.IsNullOrEmpty(_password) && !string.IsNullOrEmpty(_passwordConfirmation) && _password == _passwordConfirmation)
            {
                long accountId = _accountsService.GetAll().FirstOrDefault()?.AccountId ?? 0;

                if (accountId == 0)
                {
                    accountId = _accountsService.Create(AccountType.User, _accountInfo, _password);
                }
                else
                {
                    _accountsService.Update(accountId, _accountInfo, _password);
                }

                try
                {
                    AccountDescriptor accountDescriptor = _accountsService.GetById(accountId);

                    if (accountDescriptor != null)
                    {
                        _executionContext.InitializeUtxoExecutionServices(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey, accountDescriptor.SecretViewKey);

                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            INavigationResult navigationResult = await NavigationService.NavigateAsync("/Root/NavigationPage/MainPage").ConfigureAwait(false);
                            if (!navigationResult.Success)
                            {
                                await _pageDialogService.DisplayAlertAsync(AppResources.CAP_AUTHENTICATION_ALERT_TITLE, navigationResult.Exception.Message, AppResources.BTN_OK);
                            }
                        });
                    }
                    else
                    {
                        _pageDialogService.DisplayAlertAsync(AppResources.CAP_AUTHENTICATION_ALERT_TITLE, AppResources.CAP_AUTHENTICATION_ALERT_TEXT, AppResources.BTN_OK);
                    }
                }
                catch (Exception ex)
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_AUTHENTICATION_ALERT_TITLE, ex.Message, AppResources.BTN_OK);
                }
            }
        });
    }
}
