using Newtonsoft.Json;
using O10.Core.Serialization;
using System;

namespace O10.Core.Models
{
    /// <summary>
    /// All blocks in all types of chains must inherit from this base class
    /// </summary>
    public abstract class PacketBase : Entity, IPacket
    {
        public ulong SyncHeight { get; set; }

        public uint Nonce { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        /// <summary>
        /// 24 byte value of hash of sum of Hash of referenced Sync Block Content and Nonce
        /// </summary>
        public byte[] PowHash { get; set; }

        public abstract ushort LedgerType { get; }

        public abstract ushort Version { get; }

        public abstract ushort PacketType { get; }

        [JsonIgnore]
        /// <summary>
        /// Bytes of packet (without signature and public key)
        /// </summary>
        public Memory<byte> BodyBytes { get; set; }

        //public Memory<byte> NonHeaderBytes { get; set; }

        [JsonIgnore]
        /// <summary>
        /// All bytes of packet (without DLE + STX and length)
        /// </summary>
        public Memory<byte> RawData { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, new ByteArrayJsonConverter(), new KeyJsonConverter(), new MemoryByteJsonConverter());
        }

        public static T Create<T>(string content) where T: PacketBase
        {
            return JsonConvert.DeserializeObject<T>(content, new ByteArrayJsonConverter(), new KeyJsonConverter(), new MemoryByteJsonConverter());
        }
    }
}
