using O10.Core.Configuration;

namespace O10.Node.Core.Configuration
{
    public interface INodeConfiguration : IConfigurationSection
    {
        string[] Modules { get; set; }

        string[] CommunicationServices { get; set; }
    }
}
