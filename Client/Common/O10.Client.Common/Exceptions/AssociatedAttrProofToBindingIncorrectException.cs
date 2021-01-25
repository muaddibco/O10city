using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class AssociatedAttrProofToBindingIncorrectException : Exception
    {
        public AssociatedAttrProofToBindingIncorrectException() { }
        public AssociatedAttrProofToBindingIncorrectException(string schemeName) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_TO_BINDING, schemeName)) { }
        public AssociatedAttrProofToBindingIncorrectException(string schemeName, Exception inner) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_TO_BINDING, schemeName), inner) { }
        protected AssociatedAttrProofToBindingIncorrectException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
