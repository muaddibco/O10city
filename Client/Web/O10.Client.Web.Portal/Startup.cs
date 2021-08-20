using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using O10.Client.Web.Common.Hubs;
using O10.Core.Configuration;
using O10.Core.ExtensionMethods;
using O10.Core.Logging;
using O10.Server.IdentityProvider.Common.Controllers;
using O10.Server.IdentityProvider.Common.Hubs;
using O10.Client.Web.Portal.Hubs;
using O10.Client.Web.Portal.IdentityServer.Data;
using O10.Client.Web.Portal.Services.Inherence;
using O10.Client.Web.Saml.Common.Hubs;
using Newtonsoft.Json;
using Flurl.Http.Configuration;
using Flurl.Http;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Cyberboss.AspNetCore.AsyncInitializer;
using O10.Transactions.Core;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using O10.Core.Serialization;
using O10.Client.Web.Portal.Services;

namespace O10.Client.Web.Portal
{
    public class Startup
    {
        static IContractResolver _suppressItemTypeNameContractResolver = new SuppressItemTypeNameContractResolver();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Log4NetLogger _logger;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            _cancellationTokenSource = new CancellationTokenSource();
            _logger = new Log4NetLogger(null);
            _logger.Initialize(nameof(Startup), "log4net.xml");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddApplicationInsightsTelemetry();
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(
            //        Configuration.GetConnectionString("DefaultConnection")));

            //services.AddDefaultIdentity<ApplicationUser>(options =>
            //    {
            //        options.Password.RequireDigit = false;
            //        options.Password.RequiredLength = 3;
            //        options.Password.RequiredUniqueChars = 0;
            //        options.Password.RequireLowercase = false;
            //        options.Password.RequireNonAlphanumeric = false;
            //        options.Password.RequireUppercase = false;
            //    })
            //    .AddRoles<IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            //IIdentityServerBuilder identityServerBuilder =
            //    services.AddIdentityServer()
            //        .AddApiAuthorization<ApplicationUser, ApplicationDbContext>()
            //        .AddProfileService<ProfileService>();

            //services.AddAuthentication()
            //    .AddIdentityServerJwt();

            services.AddControllersWithViews().AddNewtonsoftJson();
            //services.AddRazorPages();

            services.AddCors(options =>
            {
                options.AddPolicy("Public", builder => {});
            }).AddTransient<ICorsPolicyAccessor, CorsPolicyAccessor>();

            services.AddMvc()
                .AddApplicationPart(typeof(IdentityProviderController).Assembly)
                //.AddApplicationPart(typeof(SamlIdpController).Assembly)
                .AddControllersAsServices()
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
                    o.SerializerSettings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    o.SerializerSettings.Formatting = Formatting.Indented;
                    o.SerializerSettings.Converters.Add(new StringEnumConverter());
                });

            // In production, the Angular files will be served from this directory
            //services.AddSpaStaticFiles(configuration =>
            //{
            //    configuration.RootPath = "ClientApp/dist";
            //});

            services.AddSignalR();
            //services.TryAddTransient<IClaimsService, ClaimsService>();

            services.AddBootstrapper<WebApiBootstrapper>(_logger);
            services.Replace(new ServiceDescriptor(typeof(IAppConfig), _ => new JsonAppConfig(Configuration), ServiceLifetime.Singleton));

            FlurlHttp.Configure(s =>
            {
                var jsonSettings = new JsonSerializerSettings 
                { 
                    TypeNameHandling = TypeNameHandling.All,
                    //ContractResolver = _suppressItemTypeNameContractResolver,
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented,
                };

                jsonSettings.Converters.Add(new StringEnumConverter());
                jsonSettings.Converters.Add(new KeyJsonConverter());
                //jsonSettings.Converters.Add(new ByteArrayJsonConverter());
                s.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app?.UseAsyncInitialization(async ct =>
            {
                await app.ApplicationServices.UseBootstrapper<WebApiBootstrapper>(ct, _logger).ConfigureAwait(false);
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            //app.UseStaticFiles();

            //if (!env.IsDevelopment())
            //{
            //    app.UseSpaStaticFiles();
            //}

            app.UseRouting();

            app.UseCors(x => x
                .WithOrigins("https://localhost:5011")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());

            //app.UseAuthentication();
            //app.UseIdentityServer();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "api/{controller}");
                //endpoints.MapRazorPages();
                //endpoints.MapControllers();

                endpoints.MapHub<IdentitiesHub>("/identitiesHub", o =>
                {
                    o.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling | Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                });

                endpoints.MapHub<ConsentManagementHub>("/consentHub", o =>
                {
                    o.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling | Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                });

                endpoints.MapHub<SamlIdpHub>("/samlIdpHub", o =>
                {
                    o.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling | Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                });

                endpoints.MapHub<NotificationsHub>("/idpNotifications", o =>
                {
                    o.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling | Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                });

                endpoints.MapHub<O10InherenceHub>("/o10InherenceHub", o =>
                {
                    o.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling | Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                });

                endpoints.MapHealthChecks("/api/Health");
            });

            //app.UseSpa(spa =>
            //{
            //    // To learn more about options for serving an Angular SPA from ASP.NET Core,
            //    // see https://go.microsoft.com/fwlink/?linkid=864501

            //    spa.Options.SourcePath = "ClientApp";

            //    if (env.IsDevelopment())
            //    {
            //        spa.UseAngularCliServer(npmScript: "start");
            //    }
            //});

            using var scope = app.ApplicationServices.CreateScope();
            scope.ServiceProvider.GetService<ApplicationDbContext>()?.Database.Migrate();
        }
    }
}
