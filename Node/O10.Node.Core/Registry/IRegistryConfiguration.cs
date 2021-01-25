using O10.Core.Configuration;

namespace O10.Node.Core.Registry
{
    public interface IRegistryConfiguration : IConfigurationSection
    {

        string TcpServiceName { get; set; }

        int ShardId { get; set; }

        int TotalNodes { get; set; }

        int Position { get; set; }
    }
}
