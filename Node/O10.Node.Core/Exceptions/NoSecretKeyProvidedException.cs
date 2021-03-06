﻿using System;
using O10.Node.Core.Properties;

namespace O10.Node.Core.Exceptions
{

    [Serializable]
    public class NoSecretKeyProvidedException : Exception
    {
        public NoSecretKeyProvidedException() : base(Resources.ERR_NO_SECRET_KEY_PROVIDED) { }
        public NoSecretKeyProvidedException(Exception inner) : base(Resources.ERR_NO_SECRET_KEY_PROVIDED, inner) { }
        protected NoSecretKeyProvidedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
