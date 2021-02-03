using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Client.DataLayer.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ClientDataContextConfiguration : ConfigurationSectionBase, IClientDataContextConfiguration
    {
        public const string SECTION_NAME = "clientDataContext";

        public ClientDataContextConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        [Tokenized]
        public string ConnectionString { get; set; }

        public string ConnectionType { get; set; }
    }
}
