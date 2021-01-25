using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Gateway.DataLayer.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class GatewayDataContextConfiguration : ConfigurationSectionBase, IGatewayDataContextConfiguration
    {
        public const string SECTION_NAME = "gatewayDataContext";

        public GatewayDataContextConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public string ConnectionString { get; set; }
        public string ConnectionType { get; set; }
	}
}
