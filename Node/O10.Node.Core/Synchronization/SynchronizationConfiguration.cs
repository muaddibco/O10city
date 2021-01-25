using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Node.Core.Synchronization
{

    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class SynchronizationConfiguration : ConfigurationSectionBase, ISynchronizationConfiguration
    {
        public const string SECTION_NAME = "sync";
        public SynchronizationConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public string CommunicationServiceName { get; set; }
        public int TotalNodes { get; set; }
        public int Position { get; set; }
    }
}
