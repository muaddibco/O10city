using O10.Core.Configuration;

namespace O10.Client.Common.Configuration
{
    public interface IRestApiConfiguration : IConfigurationSection
    {
        ushort RingSize { get; set; }
        string GatewayUri { get; set; }

        string InherenceUri { get; set; }

        string SamlIdpUri { get; set; }

        string SchemaResolutionUri { get; set; }

        string ConsentManagementUri { get; set; }

        string ExternalIdpsUri { get; set; }

        string WitnessProviderName { get; set; }

        string UniversalProofsPoolUri { get; set; }
    }
}
