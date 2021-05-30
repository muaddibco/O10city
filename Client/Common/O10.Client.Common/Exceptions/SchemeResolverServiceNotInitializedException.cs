using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class SchemeResolverServiceNotInitializedException : Exception
    {
        public SchemeResolverServiceNotInitializedException() : base(Resources.ERR_SCHEME_RESOLVER_SERVICE_NOT_INITIALIZED) { }
        public SchemeResolverServiceNotInitializedException(Exception inner) : base(Resources.ERR_SCHEME_RESOLVER_SERVICE_NOT_INITIALIZED, inner) { }
        protected SchemeResolverServiceNotInitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
