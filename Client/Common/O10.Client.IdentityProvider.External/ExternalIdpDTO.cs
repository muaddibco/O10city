using System.Collections.Generic;
using O10.Client.Common.Dtos;

namespace O10.Client.IdentityProvider.External
{
    internal class ExternalIdpDTO
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }

        public IEnumerable<AttributeDefinitionDTO> AttributeDefinitions { get; set; }
    }
}
