using System;

namespace O10.Client.Web.Portal.Exceptions
{

    [Serializable]
    public class UniversalProofsSendingFailedException : Exception
    {
        public UniversalProofsSendingFailedException() { }
        public UniversalProofsSendingFailedException(string message) : base(message) { }
        public UniversalProofsSendingFailedException(string message, Exception inner) : base(message, inner) { }
        protected UniversalProofsSendingFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
