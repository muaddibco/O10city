using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Gateway.Common.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ModularityConfiguration : ConfigurationSectionBase, IModularityConfiguration
    {
        public ModularityConfiguration(IAppConfig appConfig) : base(appConfig, "modularity")
        {
        }

        [Optional]
        public string[] Modules { get; set; }
    }
}
