using O10.Client.DataLayer.Enums;
using System;
using O10.Client.Web.Portal.Properties;

namespace O10.Client.Web.Portal.Exceptions
{

    [Serializable]
    public class UnexpectedAccountTypeException : Exception
    {
        public UnexpectedAccountTypeException() { }
        public UnexpectedAccountTypeException(long accountId, AccountType accountType) : base(string.Format(Resources.ERR_UNEXPECTED_ACCOUNT_TYPE, accountId, accountType)) { }
        public UnexpectedAccountTypeException(long accountId, AccountType accountType, Exception inner) : base(string.Format(Resources.ERR_UNEXPECTED_ACCOUNT_TYPE, accountId, accountType), inner) { }
        protected UnexpectedAccountTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
