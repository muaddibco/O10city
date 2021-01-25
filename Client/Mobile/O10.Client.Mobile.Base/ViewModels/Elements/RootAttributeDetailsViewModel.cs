namespace O10.Client.Mobile.Base.ViewModels.Elements
{
    public class RootAttributeDetailsViewModel : RootAttributeViewModel
    {
        public string IssuanceCommitment { get; set; }
        public string IssuanceCommitmentVarbinary { get; set; }
        public string OriginalCommitment { get; set; }
        public string OriginalCommitmentVarbinary { get; set; }
        public string OriginalBlindingFactor { get; set; }
        public string LastBlindingFactor { get; set; }
        public string LastCommitment { get; set; }
        public string LastTransactionKey { get; set; }
        public string LastDestinationKey { get; set; }
        public string NextKeyImage { get; set; }
        public string NextKeyImageVarbinary { get; set; }
    }
}
