namespace O10.Client.Web.Portal.Dtos.ServiceProvider
{
    public class DocumentSignatures
    {
        public long DocumentId { get; set; }

        public DocumentSignatureDto[] Signatures { get; set; }
    }
}
