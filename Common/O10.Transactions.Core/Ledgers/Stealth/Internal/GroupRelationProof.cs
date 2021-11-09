using O10.Crypto.Models;

namespace O10.Transactions.Core.Ledgers.Stealth.Internal
{
    public class GroupRelationProof
    {
        public byte[] GroupOwner { get; set; }

        public SurjectionProof RelationProof { get; set; }

        public SurjectionProof GroupNameProof { get; set; }
    }
}
