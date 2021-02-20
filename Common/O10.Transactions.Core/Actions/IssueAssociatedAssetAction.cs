namespace O10.Transactions.Core.Actions
{
    public class IssueAssociatedAssetAction : ActionBase
    {
        public override ActionType ActionType => ActionType.IssueAssociatedAsset;

        public byte[] AssetCommitment { get; set; }

        /// <summary>
        /// Contains Commitment produced from another original commitment: C` = C + r`*G
        /// EcdhTuple contains additional blinding factor r` and original commitment C
        /// </summary>
        public byte[] RootAssetCommitment { get; set; }
    }
}
