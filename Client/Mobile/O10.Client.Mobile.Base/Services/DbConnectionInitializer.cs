using System.Threading;
using O10.Client.DataLayer.Configuration;
using O10.Core;
using O10.Core.Architecture;
using O10.Core.Configuration;
using O10.Client.Mobile.Base.Interfaces;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterExtension(typeof(IInitializer), Lifetime = LifetimeManagement.Singleton)]
    public class DbConnectionInitializer : InitializerBase
    {
        private readonly IClientDataContextConfiguration _configuration;

        public DbConnectionInitializer(IConfigurationService configurationService)
        {
            _configuration = configurationService.Get<IClientDataContextConfiguration>();
        }

        public override ExtensionOrderPriorities Priority => ExtensionOrderPriorities.Highest9;

        protected override void InitializeInner(CancellationToken cancellationToken)
        {
            string databaseName = _configuration.ConnectionString.Replace("Filename=", "");
            IConnectionStringProvider connectionStringProvider = DependencyService.Get<IConnectionStringProvider>();
            _configuration.ConnectionString = $"Filename={connectionStringProvider.GetConnectionString(databaseName)}";
        }
    }
}
