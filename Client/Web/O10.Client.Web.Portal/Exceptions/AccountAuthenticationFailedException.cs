using System;
using O10.Client.Web.Portal.Properties;

namespace O10.Client.Web.Portal.Exceptions
{

    [Serializable]
    public class AccountAuthenticationFailedException : Exception
    {
        public AccountAuthenticationFailedException() { }
        public AccountAuthenticationFailedException(long accountId) : base(string.Format(Resources.ERR_ACCOUNT_AUTHENTICATION_FAILED, accountId)) { }
        public AccountAuthenticationFailedException(long accountId, Exception inner) : base(string.Format(Resources.ERR_ACCOUNT_AUTHENTICATION_FAILED, accountId), inner) { }
        protected AccountAuthenticationFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
