using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Transactional
{
    public class IssueBlindedAsset : TransactionalPacketBase
	{
		public override ushort Version => 1;

		public override ushort BlockType => ActionTypes.Transaction_IssueBlindedAsset;

        public byte[] GroupId { get; set; }

        public byte[] AssetCommitment { get; set; }

		public byte[] KeyImage { get; set; }

		public RingSignature UniquencessProof { get; set; }
	}
}
