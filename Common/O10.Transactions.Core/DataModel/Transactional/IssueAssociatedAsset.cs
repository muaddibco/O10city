using O10.Transactions.Core.DataModel.Transactional.Internal;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DataModel.Transactional
{
    public class IssueAssociatedAsset : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort BlockType => ActionTypes.Transaction_IssueAssociatedAsset;

        public AssetIssuance AssetIssuance { get; set; }
    }
}
