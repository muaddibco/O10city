using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public class IssueAssociatedBlindedAssetTransaction : O10StateTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_IssueAssociatedBlindedAsset;

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? AssetCommitment { get; set; }

        /// <summary>
        /// Contains Commitment produced from another original commitment: C` = C + r`*G
        /// EcdhTuple contains additional blinding factor r` and original commitment C
        /// </summary>
        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey? RootAssetCommitment { get; set; }
    }
}
