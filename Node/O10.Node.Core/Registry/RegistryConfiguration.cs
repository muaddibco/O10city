using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Node.Core.Registry
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryConfiguration : ConfigurationSectionBase, IRegistryConfiguration
    {
        public const string SECTION_NAME = "registry";

        public RegistryConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public string TcpServiceName { get; set; }

        public string UdpServiceName { get; set; }
        public int ShardId { get; set; }
        public int TotalNodes { get; set; }
        public int Position { get; set; }
    }
}
