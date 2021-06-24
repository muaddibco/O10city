using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class BlindedAssetsValuesGroup : BlindedAssetsGroup
    {
        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][] ValueCommitments { get; set; }
    }
}
