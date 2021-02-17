using System;
using Newtonsoft.Json;
using O10.Core.Identity;
using O10.Core.Serialization;

namespace O10.Core.Models
{
    public abstract class SignedPacketBase : PacketBase
    {
        public ulong Height { get; set; }

        [JsonConverter(typeof(KeyJsonConverter))]
        public IKey Signer { get; set; }

        [JsonConverter(typeof(MemoryByteJsonConverter))]
        public Memory<byte> Signature { get; set; }
    }
}
