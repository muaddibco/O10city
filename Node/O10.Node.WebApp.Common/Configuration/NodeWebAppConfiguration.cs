using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Node.WebApp.Common.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class NodeWebAppConfiguration : ConfigurationSectionBase
    {
        public const string SECTION_NAME = "nodeWebApp";

        public NodeWebAppConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public string AzureADCertThumbprint { get; set; }
        public string KeyVaultName { get; set; }
        public string AzureADApplicationId { get; set; }
    }
}
