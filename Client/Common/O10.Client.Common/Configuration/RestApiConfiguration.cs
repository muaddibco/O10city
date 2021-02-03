using O10.Core.Architecture;

using O10.Core.Configuration;
using O10.Core.ExtensionMethods;

namespace O10.Client.Common.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class RestApiConfiguration : ConfigurationSectionBase, IRestApiConfiguration
    {
        public const string SECTION_NAME = "RestApi";

        public RestApiConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public ushort RingSize { get; set; }

        [Tokenized]
        public string GatewayUri { get; set; }

        [Tokenized]
        public string InherenceUri { get; set; }

        [Tokenized]
        public string SamlIdpUri { get; set; }

        [Tokenized]
        public string SchemaResolutionUri { get; set; }

        [Optional, Tokenized]
        public string ExternalIdpsUri { get; set; }

        [Tokenized]
        public string ConsentManagementUri { get; set; }

        public string WitnessProviderName { get; set; }

        [Tokenized]
        public string UniversalProofsPoolUri { get; set; }
    }
}
