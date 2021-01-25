using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Client.Mobile.Base
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class MobileConfiguration : ConfigurationSectionBase, IMobileConfiguration
    {
        public const string SECTION_NAME = "mobile";

        public MobileConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public bool IsSimulator { get; set; }

        public bool IsEmulated { get; set; }
    }
}
