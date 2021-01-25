using O10.Core.Properties;
using System;

namespace O10.Core.Exceptions
{

    [Serializable]
    public class InvalidTargetPipeRequestException : Exception
    {
        public InvalidTargetPipeRequestException() { }
        public InvalidTargetPipeRequestException(Type type) : base(string.Format(Resources.ERR_INVALID_TARGET_PIPE_REQUEST, type.FullName)) { }
        public InvalidTargetPipeRequestException(Type type, Exception inner) : base(string.Format(Resources.ERR_INVALID_TARGET_PIPE_REQUEST, type.FullName), inner) { }
        protected InvalidTargetPipeRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
