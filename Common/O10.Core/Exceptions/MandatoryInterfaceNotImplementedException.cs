using System;
using O10.Core.Properties;

namespace O10.Core.Exceptions
{

    [Serializable]
    public class MandatoryInterfaceNotImplementedException : Exception
    {
        public MandatoryInterfaceNotImplementedException() { }
        public MandatoryInterfaceNotImplementedException(Type aspect, Type interfaceType, Type declaringType) : base(string.Format(Resources.ERR_MANDATORY_INTERFACE_NOT_IMPLEMENTED, aspect, interfaceType, declaringType)) { }
        public MandatoryInterfaceNotImplementedException(Type aspect, Type interfaceType, Type declaringType, Exception inner) : base(string.Format(Resources.ERR_MANDATORY_INTERFACE_NOT_IMPLEMENTED, aspect, interfaceType, declaringType), inner) { }
        protected MandatoryInterfaceNotImplementedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
