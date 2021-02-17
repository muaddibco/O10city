namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class AcceptedAssetUnblindingUtxo
    {
        public AcceptedAssetUnblinding AcceptedAssetsUnblinding { get; set; }

        /// <summary>
        /// Contains a Transaction Key specified in that transaction
        /// </summary>
        public byte[] SourceKey { get; set; }
    }
}
