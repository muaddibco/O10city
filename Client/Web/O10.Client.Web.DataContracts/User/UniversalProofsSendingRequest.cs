using Newtonsoft.Json;
using O10.Client.Common.Dtos.UniversalProofs;
using O10.Core.Identity;
using O10.Core.Serialization;
using System.Collections.Generic;

namespace O10.Client.Web.DataContracts.User
{
    public class UniversalProofsSendingRequest
    {
        public long RootAttributeId { get; set; }
        
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? Target { get; set; }
        
        public string SessionKey { get; set; }
        public UniversalProofsMission? Mission { get; set; }
        public string ServiceProviderInfo { get; set; }
        public List<IdentityPool> IdentityPools { get; set; }

        public class IdentityPool
        {
            public long RootAttributeId { get; set; }

            /// <summary>
            /// IDs of associated attributes, including attributes from associated identities (and
            /// root attribute of the associated identity too), according to validations requested
            /// by counterparty
            /// </summary>
            public List<long> AssociatedAttributes { get; set; }
        }
    }
}
