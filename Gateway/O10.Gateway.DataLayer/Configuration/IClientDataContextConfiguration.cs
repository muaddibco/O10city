using O10.Core.Configuration;

namespace O10.Gateway.DataLayer.Configuration
{
    public interface IGatewayDataContextConfiguration : IConfigurationSection
    {
        string ConnectionString { get; set; }
        string ConnectionType { get; set; }
	}
}
