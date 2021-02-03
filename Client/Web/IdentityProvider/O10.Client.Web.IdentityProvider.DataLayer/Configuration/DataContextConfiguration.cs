using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.IdentityProvider.DataLayer.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class DataContextConfiguration : ConfigurationSectionBase, IDataContextConfiguration
    {
        public const string SECTION_NAME = "o10IdpDataContext";

        public DataContextConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        [Tokenized]
        public string ConnectionString { get; set; }

        public string ConnectionType { get; set; }
	}
}
