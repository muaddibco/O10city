using O10.Client.Common.Dtos;
using O10.Client.Common.Integration;

namespace O10.Client.Web.DataContracts.SchemeResolution
{
    public class AttributeDefinitionsResponse
    {
        public AttributeDefinitionDTO[] AttributeDefinitions { get; set; }
        public ActionStatus IntegrationActionStatus { get; set; }
    }
}
