using System.Collections.Generic;

namespace O10.Client.Web.Portal.Dtos.ServiceProvider
{
    public class ServiceProviderRegistrationExDto : ServiceProviderRegistrationDto
    {
        public string Issuer { get; set; }
        public string IssuerName { get; set; }
        public string RootAttributeName { get; set; }
        public List<string> IssuanceCommitments { get; set; }
    }
}
