using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Threading;
using O10.Client.Common.Interfaces;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Services
{
    public static class BackgroundBootstrapper
    {
        public static IServiceProvider Initialize(CancellationToken cancellationToken, ILogger logger = null)
        {
            if (logger == null)
            {
                logger = new Log4NetLogger(null);
                logger.Initialize(nameof(BackgroundBootstrapper));
            }

            ServiceCollection services = new ServiceCollection();
            services.AddBootstrapper<MobileBootstrapper>(logger);
            services.RemoveAll<IAppConfig>().AddSingleton(typeof(IAppConfig), s => DependencyService.Get<IAppConfig>());
            services.RemoveAll<IHTTPClientHandlerCreationService>().AddSingleton(typeof(IHTTPClientHandlerCreationService), s => DependencyService.Get<IHTTPClientHandlerCreationService>());
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.UseBootstrapper<MobileBootstrapper>(cancellationToken, logger);

            return serviceProvider;
        }
    }
}
