using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Core.Tests.Classes
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigA : ConfigurationSectionBase
    {
        public ConfigA(IAppConfig appConfig) : base(appConfig, nameof(ConfigA))
        {
        }
        public ushort MaxValue { get; set; }
    }
}
