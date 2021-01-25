using Flurl;
using Flurl.Http;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using O10.Client.Common.Interfaces;
using O10.Client.DataLayer.Model;
using O10.Client.DataLayer.Services;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Client.Mobile.Base.ExtensionMethods;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using O10.Client.Mobile.Base.Services.Inherence;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class O10InherenceRemovalPageViewModel : ViewModelBase
    {
        private const string NAME = "O10Inherence";

        private readonly ILogger _logger;
        private readonly IO10InherenceConfiguration _o10InherenceConfiguration;
        private readonly IDataAccessService _dataAccessService;
        private readonly IVerifierInteractionsManager _verifierInteractionsManager;
        private readonly IPageDialogService _pageDialogService;
        private readonly IExecutionContext _executionContext;
        private readonly IAccountsService _accountsService;
        private string _password;
        private UserRootAttribute _rootAttribute;
        private byte[] _target;

        public O10InherenceRemovalPageViewModel(INavigationService navigationService,
                                                 IDataAccessService dataAccessService,
                                                 IVerifierInteractionsManager verifierInteractionsManager,
                                                 IConfigurationService configurationService,
                                                 ILoggerService loggerService,
                                                 IPageDialogService pageDialogService,
                                                 IExecutionContext executionContext,
                                                 IAccountsService accountsService) : base(navigationService)
        {
            _logger = loggerService.GetLogger(nameof(O10InherenceRemovalPageViewModel));
            _o10InherenceConfiguration = configurationService.Get<IO10InherenceConfiguration>();
            _dataAccessService = dataAccessService;
            _verifierInteractionsManager = verifierInteractionsManager;
            _pageDialogService = pageDialogService;
            _executionContext = executionContext;
            _accountsService = accountsService;
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
            IsLoading = true;

            var account = _accountsService.Authenticate(_executionContext.AccountId, Password);

            if (account == null)
            {
                Device.BeginInvokeOnMainThread(() => _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, AppResources.CAP_AUTHENTICATION_ALERT_TEXT, AppResources.BTN_OK));

                return;
            }

            _executionContext.RelationsBindingService.GetBoundedCommitment(_rootAttribute.AssetId, _target, out byte[] blindingFactor, out byte[] commitment);

            _o10InherenceConfiguration.Uri
                .AppendPathSegment("RegisterPerson")
                .SetQueryParam("issuer", _rootAttribute.Source)
                .SetQueryParam("commitment", commitment.ToHexString())
                .DeleteAsync()
                .ContinueWith(t =>
                {
                    IsLoading = false;
                    if (t.IsCompletedSuccessfully)
                    {
                        _logger.Debug("Removing registration at O10 Inherence succeeded");

                        _dataAccessService.RemoveUserRegistration(_executionContext.AccountId, commitment.ToHexString());

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, AppResources.CAP_O10INHERENCE_ALERT_UNREGISTER_SUCCEEDED, AppResources.BTN_OK);
                            NavigationService.NavigateToRoot(_logger);
                        });
                    }
                    else
                    {
                        _logger.Error("Failed to remove O10 Inherence registration", t.Exception.InnerException);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            _pageDialogService.DisplayAlertAsync(AppResources.CAP_O10INHERENCE_ALERT_TITLE, AppResources.CAP_O10INHERENCE_ALERT_UNREGISTER_FAILED, AppResources.BTN_OK);
                            NavigationService.NavigateToRoot(_logger);
                        });
                    }
                }, TaskScheduler.Current);
        });

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            long rootAttributeId = long.Parse(parameters["rootAttributeId"].ToString());
            _rootAttribute = _dataAccessService.GetUserRootAttribute(rootAttributeId);
            _target = _verifierInteractionsManager.GetInstance(NAME).ServiceInfo.Target.HexStringToByteArray();
        }
    }
}
