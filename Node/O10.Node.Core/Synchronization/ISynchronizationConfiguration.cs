using O10.Core.Configuration;

namespace O10.Node.Core.Synchronization
{
    public interface ISynchronizationConfiguration : IConfigurationSection
    {
        string CommunicationServiceName { get; set; }

        int TotalNodes { get; set; }

        int Position { get; set; }
    }
}
