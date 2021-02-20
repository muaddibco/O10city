namespace O10.Transactions.Core.Actions
{
    public class IssueAssetAction : ActionBase
    {
        public override ActionType ActionType => ActionType.IssueAsset;

        public byte[] AssetCommitment { get; set; }
    }
}
