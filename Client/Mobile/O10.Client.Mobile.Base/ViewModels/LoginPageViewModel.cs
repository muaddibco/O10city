using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Services;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IAccountsService _accountsService;
        private readonly IExecutionContext _executionContext;
        private readonly IPageDialogService _pageDialogService;
        private string _password;
        private long _accountId;
        private bool _isBusy;
        private string _actionDescription;

        public LoginPageViewModel(INavigationService navigationService, IDataAccessService dataAccessService, IAccountsService accountsService, IExecutionContext executionContext, IPageDialogService pageDialogService) : base(navigationService)
        {
            _dataAccessService = dataAccessService;
            _accountsService = accountsService;
            _executionContext = executionContext;
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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                SetProperty(ref _isBusy, value);
            }
        }

        public string ActionDescription
        {
            get => _actionDescription;
            set
            {
                SetProperty(ref _actionDescription, value);
            }
        }

        public DelegateCommand AuthenticateCommand => new DelegateCommand(() =>
        {
            IsBusy = true;

            Task.Run(() =>
            {
                ActionDescription = AppResources.CAP_LOGIN_ACTION_AUTHENTICATION;
                AccountDescriptor accountDescriptor = _accountsService.Authenticate(_accountId, _password);

                if (accountDescriptor != null)
                {
                    ActionDescription = AppResources.CAP_LOGIN_ACTION_INITIALIZING;

                    _executionContext.InitializeUtxoExecutionServices(accountDescriptor.AccountId, accountDescriptor.SecretSpendKey, accountDescriptor.SecretViewKey);

                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        INavigationResult navigationResult = await NavigationService.NavigateAsync("/Root/NavigationPage/MainPage").ConfigureAwait(false);
                        if (!navigationResult.Success)
                        {
                            _pageDialogService.DisplayAlertAsync(AppResources.CAP_AUTHENTICATION_ALERT_TITLE, navigationResult.Exception.Message, AppResources.BTN_OK);
                        }
                    });
                }
                else
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_AUTHENTICATION_ALERT_TITLE, AppResources.CAP_AUTHENTICATION_ALERT_TEXT, AppResources.BTN_OK);
                }
            })
            .ContinueWith(t =>
            {
                ActionDescription = string.Empty;
                IsBusy = false;

                if (t.IsFaulted)
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_AUTHENTICATION_ALERT_TITLE, t.Exception?.Message, AppResources.BTN_OK);
                }
            }, TaskScheduler.Default);
        });

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            _accountId = long.Parse(parameters["accountId"].ToString());

            Title = $"Login to {_dataAccessService.GetAccount(_accountId).AccountInfo}";
        }
    }
}
