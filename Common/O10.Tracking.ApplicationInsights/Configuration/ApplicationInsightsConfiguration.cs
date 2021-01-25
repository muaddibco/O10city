using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Tracking.ApplicationInsights.Configuration
{
	[RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
	public class ApplicationInsightsConfiguration : ConfigurationSectionBase
	{
        public const string SECTION_NAME = "applicationInsights";

		public ApplicationInsightsConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
		{
		}

		public string InstrumentationKey { get; set; }
	}
}
