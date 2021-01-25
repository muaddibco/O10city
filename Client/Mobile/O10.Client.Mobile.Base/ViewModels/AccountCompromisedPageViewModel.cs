using System.Threading.Tasks.Dataflow;
using Prism.Commands;
using Prism.Navigation;
using O10.Client.Common.Interfaces;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Models.StateNotifications;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class AccountCompromisedPageViewModel : ViewModelBase
    {
        private readonly IExecutionContext _executionContext;
        private readonly IAccountsService _accountsService;
        private readonly IStateNotificationService _stateNotificationService;

        public AccountCompromisedPageViewModel(INavigationService navigationService, IExecutionContext executionContext,
            IAccountsService accountsService, IStateNotificationService stateNotificationService) : base(navigationService)
        {
            _executionContext = executionContext;
            _accountsService = accountsService;
            _stateNotificationService = stateNotificationService;
        }

        public DelegateCommand ResetAccountCommand => new DelegateCommand(() =>
        {
            _accountsService.Delete(_executionContext.AccountId);
            _executionContext.UnregisterExecutionServices();
            _stateNotificationService.NotificationsPipe.SendAsync(new AccountResetStateNotification());
            NavigationService.NavigateAsync("/AccountCreation");
        });
    }
}
