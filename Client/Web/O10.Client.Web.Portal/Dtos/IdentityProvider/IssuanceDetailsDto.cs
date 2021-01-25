using System.Collections.Generic;

namespace O10.Client.Web.Portal.Dtos.IdentityProvider
{
    public class IssuanceDetailsDto
    {
        public IssuanceDetailsRoot RootAttribute { get; set; }
        public IEnumerable<IssuanceDetailsAssociated> AssociatedAttributes { get; set; }

        public class IssuanceDetailsRoot
        {
            public string AttributeName { get; set; }
            public string OriginatingCommitment { get; set; }
            public string AssetCommitment { get; set; }
            public string SurjectionProof{ get; set; }
        }

        public class IssuanceDetailsAssociated
        {
            public string AttributeName { get; set; }
            public string AssetCommitment { get; set; }
            public string BindingToRootCommitment { get; set; }
        }
    }
}
