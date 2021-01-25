using System;
using O10.Network.Properties;

namespace O10.Network.Exceptions
{

    [Serializable]
    public class CommunicationServiceNotSupportedException : Exception
    {
        public CommunicationServiceNotSupportedException() { }
        public CommunicationServiceNotSupportedException(string name) : base(string.Format(Resources.ERR_COMMUNICATION_SERVICE_NOT_SUPPORTED, name)) { }
        public CommunicationServiceNotSupportedException(string name, Exception inner) : base(string.Format(Resources.ERR_COMMUNICATION_SERVICE_NOT_SUPPORTED, name), inner) { }
        protected CommunicationServiceNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
