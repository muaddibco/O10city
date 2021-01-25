using System.Collections.Generic;

namespace O10.Client.Web.Portal.Dtos.User
{
    public class ServiceProviderActionAndValidationsDto
    {
        public string SpInfo { get; set; }

        public bool IsRegistered { get; set; }

        public string PublicKey { get; set; }
        public string PublicKey2 { get; set; }

        public string SessionKey { get; set; }

        public bool IsBiometryRequired { get; set; }

        public string ExtraInfo { get; set; }

        public long PredefinedAttributeId { get; set; }

        public List<string> Validations { get; set; }
    }
}
