using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using O10.Core.Configuration;
using O10.Core.Logging;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.Resx;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.ViewModels
{
    public class QrScannerPageViewModel : ViewModelBase
    {
        private readonly IActionsService _actionsService;
        private readonly IPageDialogService _pageDialogService;
        private readonly IMobileConfiguration _mobileConfiguration;
        private readonly ILogger _logger;
        private bool _isEmulated;
        private string _qrEmulatorText;

        public QrScannerPageViewModel(INavigationService navigationService, IActionsService actionsService, ILoggerService loggerService,
            IConfigurationService configurationService, IPageDialogService pageDialogService) : base(navigationService)
        {
            _mobileConfiguration = configurationService.Get<IMobileConfiguration>();
            _actionsService = actionsService;
            _pageDialogService = pageDialogService;
            _logger = loggerService.GetLogger(nameof(QrScannerPageViewModel));
            _isEmulated = _mobileConfiguration.IsEmulated;
        }

        public bool IsEmulated
        {
            get => _isEmulated;
            set
            {
                SetProperty(ref _isEmulated, value);
            }
        }

        public string QrEmulatorText
        {
            get => _qrEmulatorText;
            set
            {
                SetProperty(ref _qrEmulatorText, value);
            }
        }

        public DelegateCommand ConfirmEmulatedQrCommand => new DelegateCommand(() => ProcessScannedQR(_qrEmulatorText));

        public DelegateCommand<ZXing.Result> ScanResultCommand => new DelegateCommand<ZXing.Result>(t =>
        {
            string command = t.Text;
            ProcessScannedQR(command);
        });

        private void ProcessScannedQR(string command)
        {
            if (command.Length < 32)
            {
                return;
            }

            string navigationUri = _actionsService.ResolveAction(command);
            if (string.IsNullOrEmpty(navigationUri))
            {
                _logger.Error($"Failed to resolve command {command}");
                Device.BeginInvokeOnMainThread(() =>
                {
                    _pageDialogService.DisplayAlertAsync(AppResources.CAP_QR_SCANNER_ALERT_TITLE, AppResources.ERR_FAILED_RESOLVE_QR, AppResources.BTN_OK);
                    NavigationService.GoBackAsync();
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    INavigationResult navigationResult = await NavigationService.NavigateAsync($"../{navigationUri}").ConfigureAwait(false);

                    if (!navigationResult.Success)
                    {
                        await _pageDialogService.DisplayAlertAsync(AppResources.CAP_QR_SCANNER_ALERT_TITLE, navigationResult.Exception.Message, AppResources.BTN_OK);
                    }
                });
            }
        }
    }
}
