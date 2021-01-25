using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class NoValidationProofsException : Exception
    {
        public NoValidationProofsException() : base(Resources.ERR_NO_VALIDATION_PROOFS) { }
        public NoValidationProofsException(Exception inner) : base(Resources.ERR_NO_VALIDATION_PROOFS, inner) { }
        protected NoValidationProofsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
