using System.Collections.Generic;

namespace O10.Client.Mobile.Base.Models
{
    public class ServiceProviderActionAndValidations
    {
        public string SpInfo { get; set; }
        public bool IsRegistered { get; set; }

        public string PublicKey { get; set; }

        public string SessionKey { get; set; }

        public string ExtraInfo { get; set; }

        public bool IsBiometryRequired { get; set; }

        public List<string> Validations { get; set; }
    }
}
