using O10.Client.Common.Properties;
using System;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class IntegrationIdPRegistrationFailedException : Exception
    {
        public IntegrationIdPRegistrationFailedException() { }
        public IntegrationIdPRegistrationFailedException(string details) : base(string.Format(Resources.ERR_INTEGRATION_IDP_REGISTER_FAILED, details)) { }
        public IntegrationIdPRegistrationFailedException(string details, Exception inner) : base(string.Format(Resources.ERR_INTEGRATION_IDP_REGISTER_FAILED, details), inner) { }
        protected IntegrationIdPRegistrationFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
