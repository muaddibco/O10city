using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Core.ExtensionMethods;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using Xamarin.Essentials;
using System;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class DiscloseSecretsPageViewModel : ViewModelBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly IAccountsService _accountsService;
        private readonly IDataAccessService _dataAccessService;
        private readonly IPageDialogService _pageDialogService;
        private string _password;
        private bool _showSecrets;
        private string _secretsContent;

        public DiscloseSecretsPageViewModel(INavigationService navigationService, IExecutionContext executionContext, IAccountsService accountsService, IDataAccessService dataAccessService, IPageDialogService pageDialogService) : base(navigationService)
        {
            _executionContext = executionContext;
            _accountsService = accountsService;
            _dataAccessService = dataAccessService;
            _pageDialogService = pageDialogService;
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
            }
        }

        public bool ShowSecrets
        {
            get => _showSecrets;
            set
            {
                SetProperty(ref _showSecrets, value);
            }
        }

        public string SecretsContent
        {
            get => _secretsContent;
            set
            {
                SetProperty(ref _secretsContent, value);
            }
        }

        public DelegateCommand DiscloseSecretsCommand => new DelegateCommand(async () =>
        {
            try
            {
                AccountDescriptor accountDescriptor = _accountsService.Authenticate(_executionContext.AccountId, Password);

                if (accountDescriptor != null)
                {
                    string rawContent = $"dis://{accountDescriptor.SecretSpendKey.ToHexString()}:{accountDescriptor.SecretViewKey.ToHexString()}:{_dataAccessService.GetAccount(_executionContext.AccountId).LastAggregatedRegistrations}";
                    SecretsContent = rawContent.EncodeToString64();
                    ShowSecrets = true;
                }
                else
                {
                    await _pageDialogService.DisplayAlertAsync(AppResources.CAP_DISCLOSE_SECRETS_ALERT_TITLE, AppResources.CAP_DISCLOSE_SECRETS_ALERT_AUTH_FAILURE, AppResources.BTN_OK);
                    await NavigationService.GoBackAsync();
                }
            }
            catch (Exception ex)
            {
                await _pageDialogService.DisplayAlertAsync(AppResources.CAP_DISCLOSE_SECRETS_ALERT_TITLE, ex.Message, AppResources.BTN_OK);
            }
        });

        public DelegateCommand CopyToClipboardCommand => new DelegateCommand(async () =>
        {
            await Clipboard.SetTextAsync(SecretsContent);
            bool res = await _pageDialogService.DisplayAlertAsync(AppResources.CAP_DISCLOSE_SECRETS_ALERT_TITLE, AppResources.CAP_DISCLOSE_SECRETS_ALERT_COPY_TEXT, AppResources.BTN_YES, AppResources.BTN_NO);
            if (res)
            {
                await NavigationService.GoBackAsync();
            }
        });
    }
}
