using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Threading.Tasks.Dataflow;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Enums;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;
using O10.Client.Mobile.Base.Models.StateNotifications;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class OverrideAccountPageViewModel : ViewModelBase
    {
        private readonly IAccountsService _accountsService;
        private readonly IExecutionContext _executionContext;
        private readonly IStateNotificationService _stateNotificationService;
        private readonly ICompromizationService _compromizationService;
        private readonly IPageDialogService _pageDialogService;
        private readonly ILogger _logger;
        private string _password;
        private byte[] _secretSpendKey;
        private byte[] _secretViewKey;
        private ulong _lastCombinedBlockHeight;

        public OverrideAccountPageViewModel(INavigationService navigationService, IAccountsService accountsService,
            IExecutionContext executionContext, IStateNotificationService stateNotificationService, ICompromizationService compromizationService,
            ILoggerService loggerService, IPageDialogService pageDialogService) : base(navigationService)
        {
            _accountsService = accountsService;
            _executionContext = executionContext;
            _stateNotificationService = stateNotificationService;
            _compromizationService = compromizationService;
            _pageDialogService = pageDialogService;
            _logger = loggerService.GetLogger(nameof(OverrideAccountPageViewModel));
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
            }
        }

        public DelegateCommand ConfirmCommand => new DelegateCommand(() =>
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _pageDialogService
                .DisplayAlertAsync("Account Overriding", "Are you sure you want to override your account?", "Yes", "Cancel")
                .ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        try
                        {
                            _accountsService.Override(AccountType.User, _executionContext.AccountId, _secretSpendKey, _secretSpendKey, Password, _lastCombinedBlockHeight);

                            _stateNotificationService.NotificationsPipe.SendAsync(new AccountOverridenStateNotification());
                            _executionContext.UnregisterExecutionServices();
                            _executionContext.InitializeUtxoExecutionServices(_executionContext.AccountId, _secretSpendKey, _secretViewKey);
                            _compromizationService.IsProtectionEnabled = false;

                            Device.BeginInvokeOnMainThread(() => NavigationService.GoBackAsync());

                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Failed to override account", ex);
                        }
                    }
                });
            });
        });

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            string encoded = parameters["action"]?.ToString();
            if (!string.IsNullOrEmpty(encoded))
            {
                string decoded = encoded.DecodeUnescapedFromString64();
                string[] decodedParts = decoded.Split(":");
                _secretSpendKey = decodedParts[0].HexStringToByteArray();
                _secretViewKey = decodedParts[1].HexStringToByteArray();
                _lastCombinedBlockHeight = ulong.Parse(decodedParts[2]);
            }

        }
    }
}
