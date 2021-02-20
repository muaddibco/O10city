using System;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Properties;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class NotSupportedPacketTypeHandlerException : Exception
    {
        public NotSupportedPacketTypeHandlerException() { }
        public NotSupportedPacketTypeHandlerException(LedgerType ledgerType) : base(string.Format(Resources.ERR_NOT_SUPPORTED_PACKET_TYPE_HANDLER, ledgerType)) { }
        public NotSupportedPacketTypeHandlerException(LedgerType ledgerType, Exception inner) : base(string.Format(Resources.ERR_NOT_SUPPORTED_PACKET_TYPE_HANDLER, ledgerType), inner) { }
        protected NotSupportedPacketTypeHandlerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
