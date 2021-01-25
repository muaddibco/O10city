using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class NoSchemeDefinedException : Exception
    {
        public NoSchemeDefinedException() { }
        public NoSchemeDefinedException(string schemeName, string issuer) : base(string.Format(Resources.ERR_NO_SCHEME_OF_ISSUER, schemeName, issuer)) { }
        public NoSchemeDefinedException(string schemeName, string issuer, Exception inner) : base(string.Format(Resources.ERR_NO_SCHEME_OF_ISSUER, schemeName, issuer), inner) { }
        protected NoSchemeDefinedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
