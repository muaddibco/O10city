namespace O10.Client.Mobile.Base.Models
{
    public class UserAttributeModel
    {
        public long UserAttributeId { get; set; }

        public string SchemeName { get; set; }

        public string Source { get; set; }

        public string AssetId { get; set; }

        public string OriginalBlindingFactor { get; set; }

        public string OriginalCommitment { get; set; }

        public string OriginatingCommitment { get; set; }

        public string LastBlindingFactor { get; set; }

        public string LastCommitment { get; set; }

        public string LastTransactionKey { get; set; }

        public string LastDestinationKey { get; set; }

        public bool Validated { get; set; }

        public string Content { get; set; }

        public bool IsOverriden { get; set; }
    }
}
