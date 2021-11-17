using O10.Client.Common.Dtos;

namespace O10.Client.Web.Portal.Services.Idps
{
    internal class AttributeIssuanceDetails
    {
        public AttributeDefinitionDTO Definition { get; set; }
        public IssueAttributesRequestDTO.AttributeValue Value { get; set; }
    }
}
