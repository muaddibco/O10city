using O10.Transactions.Core.DataModel.Transactional.Internal;
using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Transactional
{
    public class TransferAssetToStealth : TransactionalTransitionalPacketBase
	{
        public override ushort Version => 1;

        public override ushort BlockType => ActionTypes.Transaction_transferAssetToStealth;

        public EncryptedAsset TransferredAsset { get; set; }

        /// <summary>
        /// Surjection Proof contains reference to AssetCommitment that is being transferred. If referenced AssetCommitment is TransferredAsset of already sent commitment so it cancels previous sending
        /// </summary>
        public SurjectionProof SurjectionProof { get; set; }
    }
}
