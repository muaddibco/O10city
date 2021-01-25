using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Node.WebApp.Common.Controllers;
using O10.Node.WebApp.Common;
using O10.Node.WebApp.Common.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using O10.Core.Configuration;

namespace O10.Node.WebApp
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
            //services.AddApplicationInsightsTelemetry("6cd34bcb-3a4a-4fa1-8fa7-a749f62ef054");
            services
                .AddControllers()
                .AddApplicationPart(typeof(NetworkController).Assembly)
                .AddControllersAsServices()
                .AddNewtonsoftJson(o => 
                {
                    o.SerializerSettings.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto;
                });

            services.AddBootstrapper<NodeWebAppBootstrapper>(_logger);
            services.Replace(new ServiceDescriptor(typeof(IAppConfig), _ => new JsonAppConfig(Configuration), ServiceLifetime.Singleton));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app?.ApplicationServices
                .UseBootstrapper<NodeWebAppBootstrapper>(_cancellationTokenSource.Token, _logger);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
