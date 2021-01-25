using O10.Core.Architecture;
using O10.Core.Architecture.Enums;
using O10.Core.Configuration;

namespace O10.Core.Tests.Classes
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ConfigB : ConfigurationSectionBase
    {
        public ConfigB(IAppConfig appConfig) : base(appConfig, nameof(ConfigB))
        {
        }

        public ushort MaxValue { get; set; }
    }
}
