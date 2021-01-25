using O10.Core.Architecture;
using O10.Core.Configuration;

namespace O10.Network.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class GeneralUdpCommunicationConfiguration : CommunicationConfigurationBase
    {
        public const string SECTION_NAME = "generalUdpCommunication";

        public GeneralUdpCommunicationConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }
    }
}
