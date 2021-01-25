using System.Collections.Generic;

namespace O10.Client.Common.Entities
{
    public class BiometricPersonDataDto
    {
        public string Issuer { get; set; }

        public string SessionKey { get; set; }

        public Dictionary<string, byte[]> Images { get; set; }
    }
}
