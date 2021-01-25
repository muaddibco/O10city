using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Stealth.Internal
{
    public class AssociatedProofs
	{
		public byte[] AssociatedAssetGroupId { get; set; }
		public SurjectionProof AssociationProofs { get; set; }
		public SurjectionProof RootProofs { get; set; }
	}
}
