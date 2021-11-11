using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Tracking.Core
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class TrackingConfiguration : ConfigurationSectionBase
    {
        const string SECTION_NAME = "tracking";

        public TrackingConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        [Optional]
        public string[] TrackingReporterNames { get; set; }
    }
}
