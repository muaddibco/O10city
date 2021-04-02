using O10.Transactions.Core.Ledgers;
using O10.Transactions.Core.Properties;
using System;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class NoTransactionException : Exception
    {
        public NoTransactionException() { }
        public NoTransactionException(IPacketBase packet) : base(string.Format(Resources.ERR_NO_TRANSACTION, packet.GetType().Name)) { }
        public NoTransactionException(IPacketBase packet, Exception inner) : base(string.Format(Resources.ERR_NO_TRANSACTION, packet.GetType().Name), inner) { }
        protected NoTransactionException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
