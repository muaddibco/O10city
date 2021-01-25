using O10.Transactions.Core.Properties;
using System;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class AccessorValidationFailedException : Exception
    {
        public AccessorValidationFailedException() { }
        public AccessorValidationFailedException(string message) : base(string.Format(Resources.ERR_ACCESSOR_VALIDATION_FAILED, message)) { }
        public AccessorValidationFailedException(string message, Exception inner) : base(string.Format(Resources.ERR_ACCESSOR_VALIDATION_FAILED, message), inner) { }
        protected AccessorValidationFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
