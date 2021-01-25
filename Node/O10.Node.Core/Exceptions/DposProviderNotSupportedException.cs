using System;
using O10.Transactions.Core.Enums;
using O10.Node.Core.Properties;

namespace O10.Node.Core.Exceptions
{

    [Serializable]
    public class DposProviderNotSupportedException : Exception
    {
        public DposProviderNotSupportedException() { }
        public DposProviderNotSupportedException(PacketType packetType) : base(string.Format(Resources.ERR_DPOS_PROVIDER_NOT_SUPPORTED, packetType)) { }
        public DposProviderNotSupportedException(PacketType packetType, Exception inner) : base(string.Format(Resources.ERR_DPOS_PROVIDER_NOT_SUPPORTED, packetType), inner) { }
        protected DposProviderNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
