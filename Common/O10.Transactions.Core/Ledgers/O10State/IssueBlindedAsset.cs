using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class IssueBlindedAsset : O10StatePacket
	{
		public override ushort Version => 1;

		public override ushort PacketType => TransactionTypes.Transaction_IssueBlindedAsset;

        public byte[] AssetCommitment { get; set; }

		public byte[] KeyImage { get; set; }

		public RingSignature UniquencessProof { get; set; }
	}
}
