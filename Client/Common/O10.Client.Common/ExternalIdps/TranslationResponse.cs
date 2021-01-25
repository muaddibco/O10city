using System.Collections.Generic;

namespace O10.Client.Common.ExternalIdps
{
    public class TranslationResponse
    {
        public string Issuer { get; set; }
        public string ActionUri { get; set; }

        public Dictionary<string, string> Attributes { get; set; }
    }
}
