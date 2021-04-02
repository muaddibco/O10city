using O10.Gateway.Common.Properties;
using O10.Transactions.Core.Enums;
using System;

namespace O10.Gateway.Common.Exceptions
{

    [Serializable]
    public class NoPacketObtainedException : Exception
    {
        public NoPacketObtainedException() { }
        public NoPacketObtainedException(LedgerType ledgerType, long aggregatedRegistrationHeight, string hash) : base(string.Format(Resources.ERR_NO_PACKET_OBTAINED, ledgerType, aggregatedRegistrationHeight, hash)) { }
        public NoPacketObtainedException(LedgerType ledgerType, long aggregatedRegistrationHeight, string hash, Exception inner) : base(string.Format(Resources.ERR_NO_PACKET_OBTAINED, ledgerType, aggregatedRegistrationHeight, hash), inner) { }
        protected NoPacketObtainedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
