using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Node.Network.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class NodeConfiguration : ConfigurationSectionBase, INodeConfiguration
    {
        public const string SECTION_NAME = "node";

        public NodeConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        [Optional]
        public string[] Modules { get; set; }

        [Optional]
        public string[] CommunicationServices { get; set; }
    }
}
