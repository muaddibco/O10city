using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using O10.Client.Common.Interfaces;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models.StateNotifications;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class SettingsPageViewModel : ViewModelBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly ICompromizationService _compromizationService;
        private readonly IAccountsService _accountsService;
        private readonly IStateNotificationService _stateNotificationService;
        private readonly IPageDialogService _pageDialogService;
        private bool _isProtectionEnabled;

        public SettingsPageViewModel(INavigationService navigationService, IExecutionContext executionContext,
            ICompromizationService compromizationService, IAccountsService accountsService,
            IStateNotificationService stateNotificationService, IPageDialogService pageDialogService)
            : base(navigationService)
        {
            _executionContext = executionContext;
            _compromizationService = compromizationService;
            _accountsService = accountsService;
            _stateNotificationService = stateNotificationService;
            _pageDialogService = pageDialogService;
            IsProtectionEnabled = _compromizationService.IsProtectionEnabled;
        }

        public DelegateCommand ResetAccountCommand => new DelegateCommand(() =>
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _pageDialogService
                .DisplayAlertAsync("Confirm Account Reset", "Are you sure you want to reset account?", "Yes", "Cancel")
                .ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        _accountsService.Delete(_executionContext.AccountId);
                        _compromizationService.IsProtectionEnabled = true;
                        _executionContext.UnregisterExecutionServices();
                        _stateNotificationService.NotificationsPipe.SendAsync(new AccountResetStateNotification());

                        Device.BeginInvokeOnMainThread(() => NavigationService.NavigateAsync("/AccountCreation"));
                    }
                }, TaskScheduler.Current);
            });
        });

        public bool IsProtectionEnabled
        {
            get => _isProtectionEnabled;
            set
            {
                SetProperty(ref _isProtectionEnabled, value);
                _compromizationService.IsProtectionEnabled = value;
            }
        }

        public DelegateCommand GoToRequiredPermissionsCommand => new DelegateCommand(() =>
        {
            Device.BeginInvokeOnMainThread(() => NavigationService.NavigateAsync("RequiredAndroidPermissions"));
        });
    }
}
