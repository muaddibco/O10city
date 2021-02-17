namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class AssetIssuance
    {
        public byte[] AssetId { get; set; }

        /// <summary>
        /// string with info about issued asset with max length of 255 bytes
        /// </summary>
        public string IssuedAssetInfo { get; set; }
    }
}
