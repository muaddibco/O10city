namespace O10.Client.Common.Interfaces.Inputs
{
    public class DocumentSignRequestInput : RequestInput
    {
        public ulong DocumentRecordHeight { get; set; }
        public byte[] DocumentHash { get; set; }
        public byte[] GroupIssuer { get; set; }
        public byte[] GroupAssetId { get; set; }
    }
}
