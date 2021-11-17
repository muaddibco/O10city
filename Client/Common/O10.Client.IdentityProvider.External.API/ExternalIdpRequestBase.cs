namespace O10.Client.IdentityProvider.External.API
{
    public abstract class ExternalIdpRequestBase
    {
        public string PublicSpendKey { get; set; }
        public string PublicViewKey { get; set; }
    }
}
