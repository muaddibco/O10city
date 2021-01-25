using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Client.Web.Portal.Scenarios.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class ScenariosConfiguration : ConfigurationSectionBase, IScenariosConfiguration
    {
        const string SECTION_NAME = "scenarios";

        public ScenariosConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public string FolderPath { get; set; }
        public string ContentBasePath { get; set; }
    }
}
