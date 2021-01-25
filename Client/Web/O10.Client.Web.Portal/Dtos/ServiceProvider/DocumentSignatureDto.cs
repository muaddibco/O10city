using O10.Client.Common.Interfaces.Outputs;

namespace O10.Client.Web.Portal.Dtos.ServiceProvider
{
    public class DocumentSignatureDto
    {
        public long DocumentId { get; set; }
        public long SignatureId { get; set; }
        public string DocumentHash { get; set; }
        public ulong DocumentRecordHeight { get; set; }
        public ulong SignatureRecordHeight { get; set; }

        public DocumentSignatureVerification SignatureVerification { get; set; }
    }
}
