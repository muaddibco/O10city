using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class BindingKeyNotInitializedException : Exception
    {
        public BindingKeyNotInitializedException() : base(Resources.ERR_BINDING_KEY_NOT_INITIALIZED) { }
        public BindingKeyNotInitializedException(Exception inner) : base(Resources.ERR_BINDING_KEY_NOT_INITIALIZED, inner) { }
        protected BindingKeyNotInitializedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
