using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Gateway.Common.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizerConfiguration : ConfigurationSectionBase, ISynchronizerConfiguration
    {
        public const string SECTION_NAME = "synchronizer";
        private string _nodeApiUri;
        private string _nodeServiceApiUri;

        public SynchronizerConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        [Tokenized]
        public string NodeApiUri { get; set; }

        [Tokenized]
        public string NodeServiceApiUri { get; set; }
    }
}
