using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class AccountNotFoundException : Exception
    {
        public AccountNotFoundException() { }
        public AccountNotFoundException(long accountId) : base(string.Format(Resources.ERR_ACCOUNT_NOT_FOUND, accountId)) { }
        public AccountNotFoundException(long accountId, Exception inner) : base(string.Format(Resources.ERR_ACCOUNT_NOT_FOUND, accountId), inner) { }
        protected AccountNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
