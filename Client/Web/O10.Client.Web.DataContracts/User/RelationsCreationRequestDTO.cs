using System.Collections.Generic;

namespace O10.Client.Web.DataContracts.User
{
    public class RelationsCreationRequestDTO : UserAttributeTransferDto
    {
        public List<string> GroupDids { get; set; }
    }
}
