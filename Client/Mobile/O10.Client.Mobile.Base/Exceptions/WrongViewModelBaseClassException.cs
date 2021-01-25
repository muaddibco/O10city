using System;
using O10.Client.Mobile.Base.Properties;

namespace O10.Client.Mobile.Base.Exceptions
{

    [Serializable]
    public class WrongViewModelBaseClassException : Exception
    {
        public WrongViewModelBaseClassException() { }
        public WrongViewModelBaseClassException(Type type) : base(string.Format(Resources.ERR_VIEW_MODEL_ATTR_WRONG_BASE, type.FullName)) { }
        public WrongViewModelBaseClassException(Type type, Exception inner) : base(string.Format(Resources.ERR_VIEW_MODEL_ATTR_WRONG_BASE, type.FullName), inner) { }
        protected WrongViewModelBaseClassException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
