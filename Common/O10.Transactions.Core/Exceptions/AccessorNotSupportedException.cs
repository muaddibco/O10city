using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Properties;
using System;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class AccessorNotSupportedException : Exception
    {
        public AccessorNotSupportedException() { }
        public AccessorNotSupportedException(LedgerType ledgerType) : base(string.Format(Resources.ERR_ACCESSOR_NOT_SUPPORTED, ledgerType.ToString())) { }
        public AccessorNotSupportedException(LedgerType ledgerType, Exception inner) : base(string.Format(Resources.ERR_ACCESSOR_NOT_SUPPORTED, ledgerType.ToString()), inner) { }
        protected AccessorNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
