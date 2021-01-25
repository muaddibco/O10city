using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Integrations.Rsk.Web.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class IntegrationConfiguration: ConfigurationSectionBase, IIntegrationConfiguration
    {
        public const string SECTION_NAME = "IntegrationRsk";

        public IntegrationConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public string RpcUri { get; set; }
        public string ContractAddress { get; set; }
    }
}
