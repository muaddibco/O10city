using System;
using O10.Client.Web.Portal.Properties;

namespace O10.Client.Web.Portal.Exceptions
{

    [Serializable]
    public class NoValueForAttributeException : Exception
    {
        public NoValueForAttributeException() { }
        public NoValueForAttributeException(string attributeName) : base(string.Format(Resources.ERR_NO_VALUE_FOR_ATTR, attributeName)) { }
        public NoValueForAttributeException(string attributeName, Exception inner) : base(string.Format(Resources.ERR_NO_VALUE_FOR_ATTR, attributeName), inner) { }
        protected NoValueForAttributeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
