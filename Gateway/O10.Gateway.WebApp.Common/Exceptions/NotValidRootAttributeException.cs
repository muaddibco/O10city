using System;
using O10.Gateway.WebApp.Common.Properties;

namespace O10.Gateway.WebApp.Common.Exceptions
{

    [Serializable]
    public class NotValidRootAttributeException : Exception
    {
        public NotValidRootAttributeException() { }
        public NotValidRootAttributeException(string commitment, string issuer) : base(string.Format(Resources.ERR_NOT_VALID_ROOT_ATTR, commitment, issuer)) { }
        public NotValidRootAttributeException(string commitment, string issuer, Exception inner) : base(string.Format(Resources.ERR_NOT_VALID_ROOT_ATTR, commitment, issuer), inner) { }
        protected NotValidRootAttributeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
