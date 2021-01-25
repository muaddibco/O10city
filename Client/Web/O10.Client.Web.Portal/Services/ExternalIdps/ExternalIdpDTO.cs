using System.Collections.Generic;
using O10.Client.Common.Entities;

namespace O10.Client.Web.Portal.Services.ExternalIdps
{
    public class ExternalIdpDTO
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }

        public IEnumerable<AttributeDefinition> AttributeDefinitions { get; set; }
    }
}
