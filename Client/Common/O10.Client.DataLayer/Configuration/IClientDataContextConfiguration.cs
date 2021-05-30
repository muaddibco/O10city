using O10.Core.Configuration;

namespace O10.Client.DataLayer.Configuration
{
    public interface IClientDataContextConfiguration : IConfigurationSection
    {
        string ConnectionString { get; set; }
        string ConnectionType { get; set; }
    }
}
