namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class AcceptedAssetUnblindingAB
    {
        public AcceptedAssetUnblinding AcceptedAssetsUnblinding { get; set; }
        
        /// <summary>
        /// Contains a hash of a sender's Public Key
        /// </summary>
        public byte[] SourceAddress { get; set; }

        public ulong SourceHeight { get; set; }
    }
}
