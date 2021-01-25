namespace O10.Client.Common.Identities
{
	public class AssociatedProofPreparation
	{
		public byte[] GroupId { get; set; }
		public byte[] Commitment { get; set; }

		public byte[] CommitmentBlindingFactor { get; set; }

		public byte[] OriginatingBlindingFactor { get; set; }

		public byte[] OriginatingAssociatedCommitment { get; set; }

		public byte[] OriginatingRootCommitment { get; set; }
	}
}
