using System.Collections.Generic;

namespace O10.Client.Web.Portal.Dtos.ServiceProvider
{
    public class DocumentDto
    {
        public long DocumentId { get; set; }

        public string DocumentName { get; set; }

        public string Hash { get; set; }

        public List<AllowedSignerDto> AllowedSigners { get; set; }

        public List<DocumentSignatureDto> Signatures { get; set; }
    }
}
