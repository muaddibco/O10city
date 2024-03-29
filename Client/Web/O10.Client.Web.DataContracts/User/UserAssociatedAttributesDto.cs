﻿using System.Collections.Generic;

namespace O10.Client.Web.DataContracts.User
{
    public class UserAssociatedAttributesDto
    {
        public UserAssociatedAttributesDto()
        {
            Attributes = new List<UserAssociatedAttributeDto>();
        }

        public string IssuerAddress { get; set; }

        public string IssuerName { get; set; }

        public List<UserAssociatedAttributeDto> Attributes { get; set; }
    }
}
