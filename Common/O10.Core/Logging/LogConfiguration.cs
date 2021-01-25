using O10.Core.Architecture;

using O10.Core.Configuration;

namespace O10.Core.Logging
{
	[RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class LogConfiguration : ConfigurationSectionBase, ILogConfiguration
    {
        public const string SECTION_NAME = "logging";

        public LogConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        [Optional]
        public bool MeasureTime { get; set; }

        [Optional]
        public string LogConfigurationFile { get; set; }

        [Optional]
        public string LoggerName { get; set; }

		[Optional]
		public string LogLevel { get; set; }
	}
}
