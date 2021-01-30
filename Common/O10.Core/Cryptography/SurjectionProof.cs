using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Core.Cryptography
{
    public class SurjectionProof
    {
        public SurjectionProof()
        {
            AssetCommitments = new byte[0][];
            Rs = new BorromeanRingSignature();
        }

        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][] AssetCommitments { get; set; }
        public BorromeanRingSignature Rs { get; set; }
    }
}
