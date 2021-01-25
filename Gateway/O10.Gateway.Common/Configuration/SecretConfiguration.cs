using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Gateway.Common.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class SecretConfiguration : ConfigurationSectionBase, ISecretConfiguration
    {
        public const string SECTION_NAME = "secret";

        public SecretConfiguration(IAppConfig appConfig) 
            : base(appConfig, SECTION_NAME)
        {
        }

        public string SecretName { get; set; }
    }
}
