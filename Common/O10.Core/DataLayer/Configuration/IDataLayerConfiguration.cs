using O10.Core.Configuration;

namespace O10.Core.DataLayer.Configuration
{
	public interface IDataLayerConfiguration : IConfigurationSection
    {

        string ConnectionString { get; set; }

		string ConnectionType { get; set; }
    }
}
