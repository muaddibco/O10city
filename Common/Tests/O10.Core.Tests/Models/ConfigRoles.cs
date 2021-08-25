using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Core.Tests.Models
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigRoles : ConfigurationSectionBase
    {
        public ConfigRoles(IAppConfig appConfig) : base(appConfig, nameof(ConfigRoles))
        {
        }

        public string[] Roles { get; set; }
    }
}
