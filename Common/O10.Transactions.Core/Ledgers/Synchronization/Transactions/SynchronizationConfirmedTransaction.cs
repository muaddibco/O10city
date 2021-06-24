using Newtonsoft.Json;
using O10.Core.Serialization;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Synchronization.Transactions
{
    public class SynchronizationConfirmedTransaction : SynchronizationTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Synchronization_ConfirmedBlock;

        public ushort Round { get; set; }

        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][] Signatures { get; set; }

        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][] PublicKeys { get; set; }
    }
}
