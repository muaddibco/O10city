using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class AssociatedProofsSourceNotFoundException : Exception
    {
        public AssociatedProofsSourceNotFoundException() { }
        public AssociatedProofsSourceNotFoundException(string schemeName) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_SOURCE_NOT_FOUND, schemeName)) { }
        public AssociatedProofsSourceNotFoundException(string schemeName, Exception inner) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_SOURCE_NOT_FOUND, schemeName), inner) { }
        protected AssociatedProofsSourceNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
