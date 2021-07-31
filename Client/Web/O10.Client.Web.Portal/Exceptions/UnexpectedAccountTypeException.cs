using System;
using O10.Client.Web.Portal.Properties;
using O10.Client.Common.Entities;

namespace O10.Client.Web.Portal.Exceptions
{

    [Serializable]
    public class UnexpectedAccountTypeException : Exception
    {
        public UnexpectedAccountTypeException() { }
        public UnexpectedAccountTypeException(long accountId, AccountTypeDTO accountType) : base(string.Format(Resources.ERR_UNEXPECTED_ACCOUNT_TYPE, accountId, accountType)) { }
        public UnexpectedAccountTypeException(long accountId, AccountTypeDTO accountType, Exception inner) : base(string.Format(Resources.ERR_UNEXPECTED_ACCOUNT_TYPE, accountId, accountType), inner) { }
        protected UnexpectedAccountTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
