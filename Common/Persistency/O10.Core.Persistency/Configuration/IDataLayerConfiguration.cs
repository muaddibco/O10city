using O10.Core.Configuration;

namespace O10.Core.Persistency.Configuration
{
    public interface IDataLayerConfiguration : IConfigurationSection
    {

        string ConnectionString { get; set; }

        string ConnectionType { get; set; }
    }
}
