using System.Collections.Generic;

namespace O10.Client.Common.Dtos
{
    public class IssueAttributesRequestDTO : IdentityBaseDataDTO
    {
        /// <summary>
        /// Key of the dictionary is schemeName
        /// </summary>
        public Dictionary<string, AttributeValue> Attributes { get; set; }

        public class AttributeValue
        {
            public string Value { get; set; }
            public byte[] BlindingPointValue { get; set; }
            public byte[]? BlindingPointRoot { get; set; }
        }
    }
}
