using System;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Properties;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class PacketTypeNotSupportedBySignatureSupportingSerializersException : Exception
    {
        public PacketTypeNotSupportedBySignatureSupportingSerializersException() { }
        public PacketTypeNotSupportedBySignatureSupportingSerializersException(LedgerType chainType) : base(string.Format(Resources.ERR_SIGNATURE_SUPPORTING_SERIALIZERS_CHAIN_TYPE_NOT_SUPPORTED, chainType)) { }
        public PacketTypeNotSupportedBySignatureSupportingSerializersException(LedgerType chainType, Exception inner) : base(string.Format(Resources.ERR_SIGNATURE_SUPPORTING_SERIALIZERS_CHAIN_TYPE_NOT_SUPPORTED, chainType), inner) { }
        protected PacketTypeNotSupportedBySignatureSupportingSerializersException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
