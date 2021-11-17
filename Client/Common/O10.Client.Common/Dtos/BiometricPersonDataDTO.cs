using System.Collections.Generic;

namespace O10.Client.Common.Dtos
{
    public class BiometricPersonDataDTO
    {
        public string Issuer { get; set; }

        public string SessionKey { get; set; }

        public Dictionary<string, byte[]> Images { get; set; }
    }
}
