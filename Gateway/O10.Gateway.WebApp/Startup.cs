using System.Threading;
using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Gateway.WebApp.Common.Controllers;
using O10.Gateway.WebApp.Common.Hubs;
using O10.Server.Gateway;
using O10.Transactions.Core;

namespace O10.Gateway.WebApp
{
    public class Startup
    {
        static IContractResolver _suppressItemTypeNameContractResolver = new SuppressItemTypeNameContractResolver();
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
            services
                .AddControllers()
                .AddApplicationPart(typeof(SynchronizationController).Assembly)
                .AddControllersAsServices()
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
                    o.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            services.AddCors();

            services.AddSignalR();
            services.AddBootstrapper<GatewayCommonBootstrapper>(_logger);
            services.Replace(new ServiceDescriptor(typeof(IAppConfig), _ => new JsonAppConfig(Configuration), ServiceLifetime.Singleton));

            FlurlHttp.Configure(s =>
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    //ContractResolver = _suppressItemTypeNameContractResolver,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                };
                jsonSettings.Converters.Add(new StringEnumConverter());
                s.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app?.ApplicationServices
                .UseBootstrapper<GatewayCommonBootstrapper>(_cancellationTokenSource.Token, _logger);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<NotificationsHub>("/notificationsHub", o =>
                {
                    //o.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling | Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                });
            });
        }
    }
}
