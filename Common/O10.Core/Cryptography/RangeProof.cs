using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Core.Cryptography
{
    public class RangeProof
    {
        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][] D { get; set; } = new byte[64][]; // N/2 digit Pedersen commitments

        public BorromeanRingSignatureEx BorromeanRingSignature { get; set; }
    }
}
