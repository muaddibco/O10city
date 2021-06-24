using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Transactions.Core.Ledgers.O10State.Internal
{
    public class BlindedAssetsGroup
    {
        public uint GroupId { get; set; }

        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][] AssetCommitments { get; set; }
    }
}
