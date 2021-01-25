using O10.Core.Configuration;

namespace O10.Gateway.Common.Configuration
{
    public interface ISynchronizerConfiguration : IConfigurationSection
    {
		string NodeApiUri { get; set; }
		string NodeServiceApiUri { get; set; }
	}
}
