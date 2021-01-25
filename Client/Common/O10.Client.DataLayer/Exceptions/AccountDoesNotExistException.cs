using System;
using O10.Client.DataLayer.Properties;

namespace O10.Client.DataLayer.Exceptions
{

    [Serializable]
    public class AccountDoesNotExistException : Exception
    {
        public AccountDoesNotExistException() { }
        public AccountDoesNotExistException(long accountId) : base(string.Format(Resources.ERR_ACCOUNT_NOT_FOUND, accountId)) { }
        public AccountDoesNotExistException(long accountId, Exception inner) : base(string.Format(Resources.ERR_ACCOUNT_NOT_FOUND, accountId), inner) { }
        protected AccountDoesNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
