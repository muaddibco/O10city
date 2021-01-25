using O10.Client.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace O10.Client.Common.Tests.Mocks
{
    public class RestApiConfiguration : IRestApiConfiguration
    {
        public ushort RingSize { get; set; }
        public string GatewayUri { get; set; }
        public string InherenceUri { get; set; }
        public string SamlIdpUri { get; set; }
        public string SchemaResolutionUri { get; set; }
        public string ConsentManagementUri { get; set; }
        public string ExternalIdpsUri { get; set; }
        public string WitnessProviderName { get; set; }
        public string UniversalProofsPoolUri { get; set; }

        public string SectionName => throw new NotImplementedException();

        public void Initialize()
        {
            
        }
    }
}
