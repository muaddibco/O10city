using System;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.Properties;

namespace O10.Node.DataLayer.Exceptions
{

    [Serializable]
    public class DataAccessServiceNotFoundException : Exception
    {
        public DataAccessServiceNotFoundException() { }
        public DataAccessServiceNotFoundException(LedgerType packetType) : base(string.Format(Resources.ERR_DATA_ACCESS_SERVICE_NOT_FOUND, packetType)) { }
        public DataAccessServiceNotFoundException(LedgerType packetType, Exception inner) : base(string.Format(Resources.ERR_DATA_ACCESS_SERVICE_NOT_FOUND, packetType), inner) { }
        protected DataAccessServiceNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
