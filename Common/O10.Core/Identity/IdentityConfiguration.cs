using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Core.Identity
{
	[RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class IdentityConfiguration : ConfigurationSectionBase, IIdentityConfiguration
    {
        public const string SECTION_NAME = "identity";

        public IdentityConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public string Provider { get; set; }
    }
}
