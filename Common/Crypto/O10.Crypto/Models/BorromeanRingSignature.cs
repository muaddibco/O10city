using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Crypto.Models
{
    public class BorromeanRingSignature
    {
        public BorromeanRingSignature()
        {
            E = new byte[32];
            S = new byte[0][];
        }

        public BorromeanRingSignature(int length)
        {
            E = new byte[32];
            S = new byte[length][];

            for (int i = 0; i < length; i++)
            {
                S[i] = new byte[32];
            }
        }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        public byte[] E { get; set; }

        [JsonProperty(ItemConverterType = typeof(ByteArrayJsonConverter))]
        public byte[][] S { get; set; }
    }
}
