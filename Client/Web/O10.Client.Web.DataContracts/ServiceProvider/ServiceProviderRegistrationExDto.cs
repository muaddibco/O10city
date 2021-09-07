using System.Collections.Generic;

namespace O10.Client.Web.DataContracts.ServiceProvider
{
    public class ServiceProviderRegistrationExDto : ServiceProviderRegistrationDto
    {
        public string SessionKey { get; set; }
        public string Issuer { get; set; }
        public string IssuerName { get; set; }
        public string RootAttributeName { get; set; }
        public List<string> IssuanceCommitments { get; set; }
    }
}
