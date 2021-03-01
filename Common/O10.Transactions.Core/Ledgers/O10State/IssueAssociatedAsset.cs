using O10.Transactions.Core.Ledgers.O10State.Internal;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class IssueAssociatedAsset : O10StatePacket
    {
        public override ushort Version => 1;

        public override ushort PacketType => TransactionTypes.Transaction_IssueAssociatedAsset;

        public AssetIssuance AssetIssuance { get; set; }
    }
}
