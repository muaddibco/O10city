using O10.Client.Common.Entities;

namespace O10.Client.Web.Portal.Services.Idps
{
    internal class AttributeIssuanceDetails
    {
        public AttributeDefinition Definition { get; set; }
        public IssueAttributesRequestDTO.AttributeValue Value { get; set; }
    }
}
