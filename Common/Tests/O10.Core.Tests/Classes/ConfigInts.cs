using O10.Core.Architecture;
using O10.Core.Architecture.Enums;
using O10.Core.Configuration;

namespace O10.Core.Tests.Classes
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigInts : ConfigurationSectionBase
    {
        public ConfigInts(IAppConfig appConfig) : base(appConfig, nameof(ConfigInts))
        {
        }

        public int[] Ints { get; set; }
    }
}
