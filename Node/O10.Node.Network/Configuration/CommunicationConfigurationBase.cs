using O10.Core.Configuration;

namespace O10.Network.Configuration
{
    public class CommunicationConfigurationBase : ConfigurationSectionBase
    {
        public CommunicationConfigurationBase(IAppConfig appConfig, string sectionName) : base(appConfig, sectionName)
        {
        }

        public string CommunicationServiceName { get; set; }

        public ushort MaxConnections { get; set; }

        public ushort ReceiveBufferSize { get; set; }

        public ushort ListeningPort { get; set; }
    }
}
