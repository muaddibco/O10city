using O10.Client.Web.Saml.Common.Properties;
using System;

namespace O10.Client.Web.Saml.Common.Exceptions
{

    [Serializable]
    public class SamlSpIsNotRegisteredException : Exception
    {
        public SamlSpIsNotRegisteredException() { }
        public SamlSpIsNotRegisteredException(string entityId) : base(string.Format(Resources.ERR_SAML_SP_NOT_REGISTERED, entityId)) { }
        public SamlSpIsNotRegisteredException(string entityId, Exception inner) : base(string.Format(Resources.ERR_SAML_SP_NOT_REGISTERED, entityId), inner) { }
        protected SamlSpIsNotRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
