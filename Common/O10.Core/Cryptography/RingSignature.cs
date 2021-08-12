using Newtonsoft.Json;
using O10.Core.Serialization;

namespace O10.Core.Cryptography
{
    public class RingSignature
    {
        public RingSignature()
        {
            C = new byte[32];
            R = new byte[32];
        }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        /// <summary>
        /// 32 byte array
        /// </summary>
        public byte[] C { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        /// <summary>
        /// 32 byte array
        /// </summary>
        public byte[] R { get; set; }
    }
}
