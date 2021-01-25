using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Stealth.Internal
{
    public class GroupRelationProof
    {
        public byte[] GroupOwner { get; set; }

        public SurjectionProof RelationProof { get; set; }

        public SurjectionProof GroupNameProof { get; set; }
    }
}
