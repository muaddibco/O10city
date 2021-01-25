using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class AssociatedAttrProofsAreMissingException : Exception
    {
        public AssociatedAttrProofsAreMissingException() { }
        public AssociatedAttrProofsAreMissingException(string schemeName) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_MISSING, schemeName)) { }
        public AssociatedAttrProofsAreMissingException(string schemeName, Exception inner) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_MISSING, schemeName), inner) { }
        protected AssociatedAttrProofsAreMissingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
