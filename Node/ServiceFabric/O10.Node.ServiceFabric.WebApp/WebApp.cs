using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace O10.Node.ServiceFabric.WebApp
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
									//.UseKestrel(opt =>
         //                           {
         //                               int port = serviceContext.CodePackageActivationContext.GetEndpoint("ServiceEndpoint").Port;
         //                               opt.Listen(IPAddress.IPv6Any, port, listenOptions =>
         //                               {
         //                                   listenOptions.UseHttps(GetCertificateFromStore());
         //                                   listenOptions.NoDelay = true;
         //                               });
         //                           })
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

        /// <summary>
        /// Finds the ASP .NET Core HTTPS development certificate in development environment. Update this method to use the appropriate certificate for production environment.
        /// </summary>
        /// <returns>Returns the ASP .NET Core HTTPS development certificate</returns>
        private static X509Certificate2 GetCertificateFromStore()
        {
            string aspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (string.Equals(aspNetCoreEnvironment, "Development", StringComparison.OrdinalIgnoreCase))
            {
                const string aspNetHttpsOid = "1.3.6.1.4.1.311.84.1.1";
                const string CNName = "CN=localhost";
                using (X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadOnly);
                    var certCollection = store.Certificates;
                    var currentCerts = certCollection.Find(X509FindType.FindByExtension, aspNetHttpsOid, true);
                    currentCerts = currentCerts.Find(X509FindType.FindByIssuerDistinguishedName, CNName, true);
                    return currentCerts.Count == 0 ? null : currentCerts[0];
                }
            }
            else
            {
                throw new NotImplementedException("GetCertificateFromStore should be updated to retrieve the certificate for non Development environment");
            }
        }
    }
}
