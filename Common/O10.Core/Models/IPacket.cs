using System;

namespace O10.Core.Models
{
    public interface IPacket
    {
        ushort LedgerType { get; }

        ushort Version { get; }

        ushort PacketType { get; }

        /// <summary>
        /// Bytes of packet (without signature and public key)
        /// </summary>
        Memory<byte> BodyBytes { get; set; }

        //Memory<byte> NonHeaderBytes { get; set; }

        /// <summary>
        /// All bytes of packet (without DLE + STX and length)
        /// </summary>
        Memory<byte> RawData { get; set; }
    }
}
