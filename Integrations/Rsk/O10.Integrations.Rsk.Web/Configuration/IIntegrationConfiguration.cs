using O10.Core.Configuration;

namespace O10.Integrations.Rsk.Web.Configuration
{
    public interface IIntegrationConfiguration : IConfigurationSection
    {
        string RpcUri { get; set; }
        string ContractAddress { get; set; }
    }
}
