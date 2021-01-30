﻿using Newtonsoft.Json;
using O10.Core.Serialization;
using System;

namespace O10.Core.Models
{
    /// <summary>
    /// All blocks in all types of chains must inherit from this base class
    /// </summary>
    public abstract class PacketBase : Entity, IPacket
    {
        public ulong SyncBlockHeight { get; set; }

        public uint Nonce { get; set; }

        [JsonConverter(typeof(ByteArrayJsonConverter))]
        /// <summary>
        /// 24 byte value of hash of sum of Hash of referenced Sync Block Content and Nonce
        /// </summary>
        public byte[] PowHash { get; set; }

        public abstract ushort PacketType { get; }

        public abstract ushort Version { get; }

        public abstract ushort BlockType { get; }

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
            return GetType().Name;
        }
    }
}
