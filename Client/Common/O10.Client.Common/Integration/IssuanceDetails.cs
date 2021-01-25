using System.Collections.Generic;

namespace O10.Client.Common.Integration
{
    public class IssuanceDetails
    {
        public IssuanceDetailsRoot RootAttribute { get; set; }
        public IEnumerable<IssuanceDetailsAssociated> AssociatedAttributes { get; set; }

        public class IssuanceDetailsRoot
        {
            public string AttributeName { get; set; }
            public byte[] OriginatingCommitment { get; set; }
            public byte[] AssetCommitment { get; set; }
            public byte[] SurjectionProof { get; set; }
        }

        public class IssuanceDetailsAssociated
        {
            public string AttributeName { get; set; }
            public byte[] AssetCommitment { get; set; }
            public byte[] BindingToRootCommitment { get; set; }
        }
    }
}
