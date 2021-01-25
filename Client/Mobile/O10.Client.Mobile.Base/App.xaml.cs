using Xamarin.Forms;
using O10.Client.Mobile.Base.Views;
using Xamarin.Forms.Xaml;
using System.Reflection;
using O10.Client.Common.Interfaces;
using System.Linq;
using Prism.Ioc;
using System;
using O10.Client.Mobile.Base.Aspects;
using Prism.Microsoft.DependencyInjection;
using O10.Core.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using O10.Client.Mobile.Base.Services;
using O10.Core.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Threading;
using Prism.Navigation;
using O10.Core.Logging;
using Prism.Mvvm;
using O10.Client.Mobile.Base.ViewModels;
using System.Globalization;
using O10.Client.Mobile.Base.Interfaces;
using System.Threading.Tasks.Dataflow;
using O10.Client.Mobile.Base.ExtensionMethods;
using Xamarin.Essentials;
using Prism.Plugin.Popups;
using System.Web;
using Flurl.Http;
using Newtonsoft.Json;
using Flurl.Http.Configuration;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace O10.Client.Mobile.Base
{
    public partial class App
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ILogger _logger;
        private readonly ActionBlock<ISynchronizerServiceBinder> _synchronizerConnectedAction;
        private IServiceProvider _serviceProvider;
        private readonly TransformBlock<string, string> _navigationPipe;
        private readonly ActionBlock<string> _navigationHandler;

        public App(ISourceBlock<ISynchronizerServiceBinder> synchronizerConnected)
        {
            _navigationPipe = new TransformBlock<string, string>(s => s);
            _navigationHandler = new ActionBlock<string>(s =>
            {
                IExecutionContext executionContext = ServiceProviderServiceExtensions.GetService<IExecutionContext>(_serviceProvider);
                executionContext.NavigationPipe.SendAsync(s);
            });

            _synchronizerConnectedAction = new ActionBlock<ISynchronizerServiceBinder>(s => s.Initialize(_serviceProvider));

            synchronizerConnected.LinkTo(_synchronizerConnectedAction);
        }

        protected override void Initialize()
        {
            _logger = new Log4NetLogger(null);
            _logger.Initialize(GetType().Name);


            FlurlHttp.Configure(s =>
            {
                var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                s.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
            });

            base.Initialize();
        }

        protected override void OnStart()
        {
            try
            {
                _serviceProvider = PrismContainerExtension.Current.Instance;
                _serviceProvider.UseBootstrapper<MobileBootstrapper>(_cancellationTokenSource.Token);

                _navigationPipe.LinkTo(_navigationHandler);

                var androidSystemService = DependencyService.Get<IAndroidSystemService>();
                if (!Preferences.Get("AutoStartConfigured", false) || !androidSystemService.IsOverflowSettingsAllowed())
                {
                    NavigationService.NavigateWithLogging("/RequiredAndroidPermissions", _logger);
                }
                else
                {
                    NavigationService.NavigateByAccountStatus(_serviceProvider, _logger);
                }

                _logger.Info("Main page shown");

            }
            catch (Exception ex)
            {
                _logger.Error("Failed to start application", ex);
                throw;
            }
        }

        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            if (uri?.Host.EndsWith("o10demo.azurewebsites.net", StringComparison.OrdinalIgnoreCase) ?? false)
            {
                var query = HttpUtility.ParseQueryString(uri.Query);
                string sessionKey = query.Get("sk");
                string arg = $"{uri.Scheme}://{uri.Host}/IdentityProvider/RegistrationDetails/{sessionKey}";
                string navigationUri = "O10IdpRegistration?action=" + arg.EncodeToEscapedString64();

                _navigationPipe.SendAsync(navigationUri);
            }
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        protected override void TrackError(Exception ex, string fromEvent, object errorObject = null)
        {
            base.TrackError(ex, fromEvent, errorObject);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var services = containerRegistry.ServiceCollection();

            try
            {
                services.AddBootstrapper<MobileBootstrapper>(_logger); //, O10.Core.Architecture.Enums.RunMode.Simulator);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to start bootstrapper", ex);
                throw;
            }

            try
            {
                services.RemoveAll<IAppConfig>().AddSingleton(typeof(IAppConfig), s => DependencyService.Get<IAppConfig>());
                services.RemoveAll<IHTTPClientHandlerCreationService>().AddSingleton(typeof(IHTTPClientHandlerCreationService), s => DependencyService.Get<IHTTPClientHandlerCreationService>());
                services.RemoveAll<INavigationService>().AddTransient(typeof(INavigationService), typeof(ErrorReportingNavigationService));
                containerRegistry.RegisterOnce<NavigationPage>();
                containerRegistry.RegisterOnce<MainMasterDetailPage>();
                containerRegistry.RegisterOnce<MainMasterDetailPageMaster>();
                containerRegistry.RegisterOnce<MainMasterDetailPageMasterViewModel>();
                containerRegistry.RegisterOnce<MainMasterDetailPageMasterMenuItem>();

                containerRegistry.RegisterForNavigation<MainMasterDetailPage>("Root");

                containerRegistry.RegisterForNavigation<NavigationPage>();
                containerRegistry.RegisterPopupNavigationService();
                containerRegistry.RegisterPopupDialogService();

                foreach (
                    var item in
                    GetType().Assembly.GetTypes()
                    .Select(t => new Tuple<Type, NavigatableAttribute>(t, (NavigatableAttribute)t.GetCustomAttributes(typeof(NavigatableAttribute), true).FirstOrDefault()))
                    .Where(t => t.Item2 != null))
                {
                    _logger.Info($"Registering View {item.Item1.FullName} and its view model");
                    containerRegistry.RegisterForNavigation(item.Item1, item.Item2.Alias);
                    Type viewModelType = GetViewModelType(item.Item1);
                    containerRegistry.RegisterOnce(item.Item1);
                    containerRegistry.RegisterOnce(viewModelType);
                    _logger.Info($"Registered View {item.Item1.FullName} and ViewModel {viewModelType.FullName}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failure during RegisterTypes execution", ex);
            }
        }

        protected override void OnInitialized()
        {
            InitializeComponent();
        }

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory((view, type) =>
            {
                IServiceProvider serviceProvider = PrismContainerExtension.Current.Instance;
                INavigationService navigationService = null;
                switch (view)
                {
                    case Page page:
                        navigationService = CreateNavigationService(page);
                        break;
                    case BindableObject bindable:
                        if (bindable.GetValue(ViewModelLocator.AutowirePartialViewProperty) is Page attachedPage)
                        {
                            navigationService = CreateNavigationService(attachedPage);
                        }
                        break;
                }

                object obj = Container.Resolve(type);

                if (obj is ViewModelBase viewModelBase)
                {
                    viewModelBase.NavigationService = navigationService;
                }

                return obj;
            });
        }

        protected override IContainerExtension CreateContainerExtension()
        {
            if (_serviceProvider == null)
            {
                return base.CreateContainerExtension();
            }
            else
            {
                IContainerExtension containerExtension = ServiceProviderServiceExtensions.GetService<IContainerExtension>(_serviceProvider);
                return containerExtension;
            }
        }

        private Type GetViewModelType(Type viewType)
        {
            var viewName = viewType.FullName;
            viewName = viewName.Replace(".Views.", ".ViewModels.");
            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            var suffix = viewName.EndsWith("View") ? "Model" : "ViewModel";
            var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}{1}, {2}", viewName, suffix, viewAssemblyName);
            return Type.GetType(viewModelName);
        }
    }
}
