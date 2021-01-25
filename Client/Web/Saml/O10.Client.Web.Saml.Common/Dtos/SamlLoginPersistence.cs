using System;

namespace O10.Client.Web.Saml.Common.Dtos
{
    public class SamlLoginPersistence
    {
        public DateTime CreationTime { get; set; }
        public string RegistrationCommitment { get; set; }
        public string SingleLogoutUri { get; set; }
    }
}
