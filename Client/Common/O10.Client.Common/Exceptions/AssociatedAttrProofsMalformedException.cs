using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class AssociatedAttrProofsMalformedException : Exception
    {
        public AssociatedAttrProofsMalformedException() { }
        public AssociatedAttrProofsMalformedException(string schemeName) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_MALFORMED, schemeName)) { }
        public AssociatedAttrProofsMalformedException(string schemeName, Exception inner) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_MALFORMED, schemeName), inner) { }
        protected AssociatedAttrProofsMalformedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

}
