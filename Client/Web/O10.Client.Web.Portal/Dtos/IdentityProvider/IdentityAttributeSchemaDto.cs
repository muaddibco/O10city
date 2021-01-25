using System.Collections.Generic;

namespace O10.Client.Web.Portal.Dtos.IdentityProvider
{
    public class IdentityAttributeSchemaDto
    {
        public string Alias { get; set; }

        public string AttributeName { get; set; }

        public List<IdentityAttributeValidationSchemaDto> AvailableValidations { get; set; }
    }
}
