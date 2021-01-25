using System;
using O10.Transactions.Core.Properties;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class IdentityNotFoundException : Exception
    {
        public IdentityNotFoundException() { }
        public IdentityNotFoundException(string hashValue) : base(string.Format(Resources.ERR_IDENTITY_NOT_FOUND, hashValue)) { }
        public IdentityNotFoundException(string hashValue, Exception inner) : base(string.Format(Resources.ERR_IDENTITY_NOT_FOUND, hashValue), inner) { }
        protected IdentityNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
