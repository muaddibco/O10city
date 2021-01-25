using O10.Core.Architecture;

using O10.Core.Configuration;
using O10.Core.ExtensionMethods;

namespace O10.Client.Common.Configuration
{
    [RegisterExtension(typeof(IConfigurationSection), Lifetime = LifetimeManagement.Singleton)]
    public class RestApiConfiguration : ConfigurationSectionBase, IRestApiConfiguration
    {
        public const string SECTION_NAME = "RestApi";
        private string _gatewayUri;
        private string _inherenceUri;
        private string _samlIdpUri;
        private string _schemaResolutionUri;
        private string _externalIdpsUri;
        private string _consentManagementUri;
        private string _universalProofsPoolUri;

        public RestApiConfiguration(IAppConfig appConfig) : base(appConfig, SECTION_NAME)
        {
        }

        public ushort RingSize { get; set; }

        public string GatewayUri
        {
            get => AppConfig.ReplaceToken(_gatewayUri);
            set => _gatewayUri = value;
        }

        public string InherenceUri
        {
            get => AppConfig.ReplaceToken(_inherenceUri);
            set => _inherenceUri = value;
        }

        public string SamlIdpUri
        {
            get => AppConfig.ReplaceToken(_samlIdpUri);
            set => _samlIdpUri = value;
        }

        public string SchemaResolutionUri
        {
            get => AppConfig.ReplaceToken(_schemaResolutionUri);
            set => _schemaResolutionUri = value;
        }

        [Optional]
        public string ExternalIdpsUri
        {
            get => AppConfig.ReplaceToken(_externalIdpsUri);
            set => _externalIdpsUri = value;
        }

        public string ConsentManagementUri
        {
            get => AppConfig.ReplaceToken(_consentManagementUri);
            set => _consentManagementUri = value;
        }

        public string WitnessProviderName { get; set; }

        public string UniversalProofsPoolUri 
        { 
            get => AppConfig.ReplaceToken(_universalProofsPoolUri); 
            set => _universalProofsPoolUri = value; 
        }
    }
}
