using System.Collections.Generic;
using System.Fabric;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace O10.Gateway.ServiceFabric.WebApp
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class WebApp : StatelessService
    {
		private readonly ConfigurationPackage _configurationPackage;

		public WebApp(StatelessServiceContext context)
            : base(context)
        {
			_configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
		}

		/// <summary>
		/// Optional override to create listeners (like tcp, http) for this service instance.
		/// </summary>
		/// <returns>The collection of listeners.</returns>
		protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[]
            {
                new ServiceInstanceListener(serviceContext =>
                    new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                    {
                        ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                        return new WebHostBuilder()
									.UseKestrel()
                                    .ConfigureServices(
                                        services => services
                                            .AddSingleton(serviceContext)
											.AddSingleton(_configurationPackage))
									.UseContentRoot(Directory.GetCurrentDirectory())
                                    .UseStartup<Startup>()
                                    .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                                    .UseUrls(url)
                                    .Build();
                    }))
            };
        }
    }
}
