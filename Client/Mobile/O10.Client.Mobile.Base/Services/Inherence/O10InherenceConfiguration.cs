using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Client.Mobile.Base.Services.Inherence
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class O10InherenceConfiguration : ConfigurationSectionBase, IO10InherenceConfiguration
    {
        public const string SECTION_NAME = "O10Inherence";

        public O10InherenceConfiguration(IAppConfig appConfig)
            : base(appConfig, SECTION_NAME)
        {
        }

        public string Uri { get; set; }
    }
}
