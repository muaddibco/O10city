using System.Collections.Generic;

namespace O10.Client.Web.Portal.Dtos.User
{
    public class RelationsCreationRequestDTO : UserAttributeTransferDto
    {
        public List<string> GroupDids { get; set; }
    }
}
