using Newtonsoft.Json;
using O10.Core.Serialization;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Synchronization.Transactions
{
    public class AggregatedRegistrationsTransaction : SynchronizationTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Synchronization_RegistryCombinationBlock;

        public long SyncHeight { get; set; }

        /// <summary>
        /// Hashes of the <see cref="Registry.RegistryPacket"/>  with <see cref="Registry.Transactions.FullRegistryTransaction"/> body
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][] BlockHashes { get; set; }
    }
}
