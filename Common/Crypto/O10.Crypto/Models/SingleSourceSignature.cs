using Newtonsoft.Json;
using O10.Core.Serialization;
using System;

namespace O10.Crypto.Models
{
    public class SingleSourceSignature : SignatureBase
    {
        [JsonConverter(typeof(MemoryByteJsonConverter))]
        public Memory<byte> Signature { get; set; }
    }
}
