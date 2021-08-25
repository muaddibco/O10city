using Newtonsoft.Json;
using O10.Core.Serialization;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State.Transactions
{
    public class DocumentRecordTransaction : O10StateTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Transaction_DocumentRecord;

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[]? DocumentHash { get; set; }

        /// <summary>
        /// Contains commitments to Asset IDs of Groups that document signer must belong to one of them
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][]? AllowedSignerGroupCommitments { get; set; }
    }
}
