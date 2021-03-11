using O10.Core.Cryptography;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers.O10State.Internal;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public class TransferAssetToStealthTransaction : O10StateTransitionalTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_TransferAssetToStealth;

        public EncryptedAsset? TransferredAsset { get; set; }

        /// <summary>
        /// Surjection Proof contains reference to AssetCommitment that is being transferred. If referenced AssetCommitment is TransferredAsset of already sent commitment so it cancels previous sending
        /// </summary>
        public SurjectionProof? SurjectionProof { get; set; }
    }
}
