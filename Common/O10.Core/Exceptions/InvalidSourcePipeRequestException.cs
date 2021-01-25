using O10.Core.Properties;
using System;

namespace O10.Core.Exceptions
{

    [Serializable]
    public class InvalidSourcePipeRequestException : Exception
    {
        public InvalidSourcePipeRequestException() { }
        public InvalidSourcePipeRequestException(Type type) : base(string.Format(Resources.ERR_INVALID_SOURCE_PIPE_REQUEST, type.FullName)) { }
        public InvalidSourcePipeRequestException(Type type, Exception inner) : base(string.Format(Resources.ERR_INVALID_SOURCE_PIPE_REQUEST, type.FullName), inner) { }
        protected InvalidSourcePipeRequestException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
