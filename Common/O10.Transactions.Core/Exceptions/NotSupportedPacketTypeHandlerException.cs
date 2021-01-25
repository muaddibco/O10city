using System;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Properties;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class NotSupportedPacketTypeHandlerException : Exception
    {
        public NotSupportedPacketTypeHandlerException() { }
        public NotSupportedPacketTypeHandlerException(PacketType packetType) : base(string.Format(Resources.ERR_NOT_SUPPORTED_PACKET_TYPE_HANDLER, packetType)) { }
        public NotSupportedPacketTypeHandlerException(PacketType packetType, Exception inner) : base(string.Format(Resources.ERR_NOT_SUPPORTED_PACKET_TYPE_HANDLER, packetType), inner) { }
        protected NotSupportedPacketTypeHandlerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
