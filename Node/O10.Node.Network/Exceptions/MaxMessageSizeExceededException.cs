﻿using System;
using O10.Network.Properties;

namespace O10.Network.Exceptions
{

    [Serializable]
    public class MaxMessageSizeExceededException : Exception
    {
        public MaxMessageSizeExceededException() : base(Resources.ERR_MAX_MESSAGE_SIZE_EXCEEDED) { }
        public MaxMessageSizeExceededException(Exception inner) : base(Resources.ERR_MAX_MESSAGE_SIZE_EXCEEDED, inner) { }
        protected MaxMessageSizeExceededException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
