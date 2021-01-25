using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Server.IdentityProvider.Common.Configuration
{
	[RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class O10IdpConfiguration : ConfigurationSectionBase, IO10IdpConfiguration
    {
        public const string SECTION_NAME = "O10Idp";

        public O10IdpConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public long SessionTimeout { get; set; }
    }
}
