using O10.Transactions.Core.Ledgers.O10State.Internal;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class IssueAsset : O10StatePacket
    {
        public override ushort Version => 1;

        public override ushort PacketType => TransactionTypes.Transaction_IssueAsset;

        /// <summary>
        /// can be real asset id like social security number or asset id that is pseudonym of real asset that is mapped in external database
        /// </summary>
        public AssetIssuance AssetIssuance { get; set; }
    }
}
