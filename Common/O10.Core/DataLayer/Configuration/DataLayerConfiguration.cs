using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Core.DataLayer.Configuration
{
	[RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class DataLayerConfiguration : ConfigurationSectionBase, IDataLayerConfiguration
    {
        public const string SECTION_NAME = "dataLayer";

        public DataLayerConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        [Tokenized]
        public string ConnectionString { get; set; }

		public string ConnectionType { get; set; }
	}
}
