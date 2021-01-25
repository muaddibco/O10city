using System;
using O10.Node.Core.Properties;

namespace O10.Node.Core.Exceptions
{

    [Serializable]
    public class SecretKeyInvalidException : Exception
    {
        public SecretKeyInvalidException() : base(Resources.ERR_SECRET_KEY_INVALID) { }
        public SecretKeyInvalidException(Exception inner) : base(Resources.ERR_SECRET_KEY_INVALID, inner) { }
        protected SecretKeyInvalidException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
