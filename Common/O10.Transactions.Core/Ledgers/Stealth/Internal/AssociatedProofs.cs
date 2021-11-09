using O10.Crypto.Models;

namespace O10.Transactions.Core.Ledgers.Stealth.Internal
{
    public class AssociatedProofs
    {
        public string SchemeName { get; set; }
        public SurjectionProof AssociationProofs { get; set; }
        public SurjectionProof RootProofs { get; set; }
    }
}
