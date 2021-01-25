using O10.Core.Configuration;

namespace O10.Core.Logging
{
    public interface ILogConfiguration : IConfigurationSection
    {
        bool MeasureTime { get; set; }

        string LogConfigurationFile { get; set; }

        string LoggerName { get; set; }

		string LogLevel { get; set; }
    }
}
