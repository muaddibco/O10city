using System.Collections.Generic;
using O10.Client.DataLayer.Enums;

namespace O10.Client.Common.Identities
{
    public class IdentityAttributeValidationDescriptor
    {
        public string SchemeName { get; set; }
        public string SchemeAlias { get; set; }

        public ValidationType ValidationType { get; set; }
        public string ValidationTypeName { get; set; }

        public List<string> ValidationCriterionTypes { get; set; }
    }
}
