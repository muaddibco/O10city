using O10.Transactions.Core.DataModel.Transactional.Internal;
using O10.Transactions.Core.Enums;
using O10.Core.Cryptography;

namespace O10.Transactions.Core.DataModel.Transactional
{
    /// <summary>
    /// This transaction intended for cases when once sent asset is required to resend to another destination. This transaction in fact cancels previous ownership transfer.
    /// </summary>
    public class RetransferAssetToStealth : TransactionalTransitionalPacketBase
	{
        public override ushort Version => 1;

        public override ushort BlockType => ActionTypes.Transaction_RetransferAssetToStealth;

        public EncryptedAsset TransferedAsset { get; set; }

        /// <summary>
        /// Surjection Proof contains reference to AssetCommitment that is being transferred. If referenced AssetCommitment is TransferredAsset of already sent commitment so it cancels previous sending
        /// </summary>
        public SurjectionProof SurjectionProof { get; set; }
    }
}
