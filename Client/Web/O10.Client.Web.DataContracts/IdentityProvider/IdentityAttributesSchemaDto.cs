using System.Collections.Generic;

namespace O10.Client.Web.DataContracts.IdentityProvider
{
    public class IdentityAttributesSchemaDto
    {
        public IdentityAttributeSchemaDto RootAttribute { get; set; }

        public List<IdentityAttributeSchemaDto> AssociatedAttributes { get; set; }
    }
}
