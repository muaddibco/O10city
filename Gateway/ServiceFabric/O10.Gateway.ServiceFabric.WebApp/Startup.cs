using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Gateway.WebApp.Common.Controllers;
using O10.Gateway.WebApp.Common.Hubs;

namespace O10.Gateway.ServiceFabric.WebApp
{
    public class Startup
	{
		private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Log4NetLogger _logger;

        public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			_cancellationTokenSource = new CancellationTokenSource();
            _logger = new Log4NetLogger(null);
            _logger.Initialize(nameof(Startup), "log4net.xml");
        }

        public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
            //services.AddApplicationInsightsTelemetry("8fa6fcb4-869c-42d5-8b6c-fb4eff361eee");
            services
                .AddMvcCore()
				.AddApplicationPart(typeof(SynchronizationController).Assembly)
				.AddControllersAsServices();
            services.AddCors();

            services.AddSignalR();
            services.AddBootstrapper<GatewayServiceFabricBootstrapper>(_logger);
            services.Replace(new ServiceDescriptor(typeof(IAppConfig), typeof(ServiceFabricAppConfig), ServiceLifetime.Singleton));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
            app?.ApplicationServices
                .UseBootstrapper<GatewayServiceFabricBootstrapper>(_cancellationTokenSource.Token, _logger);

            if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseRouting();
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
            
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");

                endpoints.MapHub<NotificationsHub>("/notificationsHub", o =>
                {
                    //o.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling | Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                });
            });
		}
	}
}
