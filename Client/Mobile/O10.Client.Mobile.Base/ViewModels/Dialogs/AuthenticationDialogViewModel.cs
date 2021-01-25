using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using O10.Client.Common.Entities;
using O10.Client.Common.Interfaces;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using Xamarin.Forms;
using O10.Core.Logging;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class AuthenticationDialogViewModel : ViewModelBase, INavigationAware
    {
        private readonly IPageDialogService _pageDialogService;
        private readonly IAccountsService _accountsService;
        private readonly IExecutionContext _executionContext;
        private readonly ILogger _logger;
        private string _password;
        private string _key;
        private string _uri;
        private bool _updateLastExpanded;

        public AuthenticationDialogViewModel(INavigationService navigationService, IPageDialogService pageDialogService,
            IAccountsService accountsService, IExecutionContext executionContext, ILoggerService loggerService)
            : base(navigationService)
        {
            _pageDialogService = pageDialogService;
            _accountsService = accountsService;
            _executionContext = executionContext;
            _logger = loggerService.GetLogger(nameof(AuthenticationDialogViewModel));
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
            }
        }

        public DelegateCommand AuthenticateCommand => new DelegateCommand(() =>
        {
            IsLoading = true;
            ActionDescription = AppResources.CAP_AUTH_AUTHENTICATING;
            AccountDescriptor accountDescriptor = _accountsService.Authenticate(_executionContext.AccountId, _password);
            if (accountDescriptor != null)
            {
                ActionDescription = AppResources.CAP_AUTH_SETTING_BK;
                _executionContext.GenerateBindingKey(_key, Password);
                if (_updateLastExpanded)
                {
                    _executionContext.LastExpandedKey = _key;
                }

                ActionDescription = string.Empty;
                IsLoading = false;
                Device.BeginInvokeOnMainThread(() => NavigationService.GoBackAsync(!string.IsNullOrEmpty(_uri) ? new NavigationParameters($"redirectUri={_uri}") : null));
            }
            else
            {
                ActionDescription = string.Empty;
                IsLoading = false;

                Device.BeginInvokeOnMainThread(() =>
                {
                    NavigationService.GoBackAsync();
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_AUTHENTICATION_ALERT_TITLE, AppResources.CAP_AUTHENTICATION_ALERT_TEXT, AppResources.BTN_OK);
                });
            }
        });

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            _key = parameters.GetValue<string>("key");
            _uri = parameters.GetValue<string>("redirectUri");
            _updateLastExpanded = parameters.GetValue<string>("updateLastExpanded")?.Equals("true", System.StringComparison.InvariantCultureIgnoreCase) ?? false;
        }
    }
}
