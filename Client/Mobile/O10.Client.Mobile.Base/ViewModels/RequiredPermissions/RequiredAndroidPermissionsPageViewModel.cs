using Prism.Commands;
using Prism.Navigation;
using System;
using O10.Client.Mobile.Base.Interfaces;
using O10.Client.Mobile.Base.ExtensionMethods;
using Xamarin.Essentials;
using Xamarin.Forms;
using O10.Core.Logging;
using System.Threading.Tasks;

namespace O10.Client.Mobile.Base.ViewModels.RequiredPermissions
{
    public class RequiredAndroidPermissionsPageViewModel : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IAndroidSystemService _androidSystemService;
        private readonly ILogger _logger;

        private bool _isAutoStartConfigured;
        private bool _isOverflowWindowAllowed;
        private bool _isAtStart;
        private readonly bool _navigating;

        public RequiredAndroidPermissionsPageViewModel(INavigationService navigationService, IServiceProvider serviceProvider, ILoggerService loggerService) : base(navigationService)
        {
            _androidSystemService = DependencyService.Get<IAndroidSystemService>();
            _serviceProvider = serviceProvider;
            _logger = loggerService.GetLogger(nameof(RequiredAndroidPermissionsPageViewModel));
            IsLoading = false;
        }

        public bool IsAutoStartConfigured
        {
            get => _isAutoStartConfigured;
            set
            {
                Preferences.Set("AutoStartConfigured", value);
                SetProperty(ref _isAutoStartConfigured, value);
            }
        }

        public bool IsOverflowWindowAllowed
        {
            get => _isOverflowWindowAllowed;
            set
            {
                SetProperty(ref _isOverflowWindowAllowed, value);
            }
        }

        public DelegateCommand OpenAutoStartSettingsCommand => new DelegateCommand(() => _androidSystemService.OpenAutoStartSettings());

        public DelegateCommand OpenOverflowSettingsCommand => new DelegateCommand(async () =>
        {
            IsOverflowWindowAllowed = await _androidSystemService.OpenOverflowSettings();

            if (IsOverflowWindowAllowed && IsAutoStartConfigured)
            {
                SkipCommand.Execute();
            }
        });

        public DelegateCommand SkipCommand => new DelegateCommand(async () =>
        {
            IsLoading = true;

            await Task.Delay(250)
                    .ContinueWith(t =>
                    {
                        Device.BeginInvokeOnMainThread(() => NavigationService.NavigateByAccountStatus(_serviceProvider, _logger));
                    }, TaskScheduler.Current)
                    .ConfigureAwait(false);
        });

        public DelegateCommand RefreshCommand => new DelegateCommand(() =>
        {
            IsAutoStartConfigured = Preferences.Get("AutoStartConfigured", false);
            IsOverflowWindowAllowed = _androidSystemService.IsOverflowSettingsAllowed();
        });

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            if (parameters.ContainsKey("isAtStart"))
            {
                _isAtStart = parameters["isAtStart"]?.ToString().ToLower() == "true";
            }
            else
            {
                _isAtStart = false;
            }
        }
    }
}
