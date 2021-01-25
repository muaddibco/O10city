namespace O10.Client.Common.ExternalIdps.BlinkId
{
    public abstract class ExternalIdpRequestBase
    {
        public string PublicSpendKey { get; set; }
        public string PublicViewKey { get; set; }
    }
}
