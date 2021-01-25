using O10.Core.Configuration;

namespace O10.IdentityProvider.DataLayer.Configuration
{
    public interface IDataContextConfiguration : IConfigurationSection
    {
        string ConnectionString { get; set; }
        string ConnectionType { get; set; }
	}
}
