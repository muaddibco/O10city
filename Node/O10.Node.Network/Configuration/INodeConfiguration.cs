using O10.Core.Configuration;

namespace O10.Node.Network.Configuration
{
    public interface INodeConfiguration : IConfigurationSection
    {
        string[] Modules { get; set; }

        string[] CommunicationServices { get; set; }
    }
}
