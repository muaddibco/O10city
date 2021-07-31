using System.Collections.Generic;

namespace O10.Client.Web.DataContracts.User
{
    public class UserAttributeSchemeDto
    {
        public UserAttributeSchemeDto()
        {
            RootAttributes = new List<UserAttributeDto>();
            AssociatedSchemes = new List<UserAssociatedAttributesDto>();
        }

        public AttributeState State { get; set; }
        
        public string IssuerAddress { get; set; }
        
        public string IssuerName { get; set; }
        
        public string RootAttributeContent { get; set; }
        
        public string RootAssetId { get; set; }
        
        public string SchemeName { get; set; }
        
        public List<UserAttributeDto> RootAttributes { get; }

        public List<UserAssociatedAttributesDto> AssociatedSchemes { get; }
    }
}
