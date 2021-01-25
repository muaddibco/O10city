using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class AssociatedAttrProofToValueKnowledgeIncorrectException : Exception
    {
        public AssociatedAttrProofToValueKnowledgeIncorrectException() { }
        public AssociatedAttrProofToValueKnowledgeIncorrectException(string schemeName) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_TO_VALUE_KNOWLEDGE, schemeName)) { }
        public AssociatedAttrProofToValueKnowledgeIncorrectException(string schemeName, Exception inner) : base(string.Format(Resources.ERR_ASSOCIATED_PROOF_TO_VALUE_KNOWLEDGE, schemeName), inner) { }
        protected AssociatedAttrProofToValueKnowledgeIncorrectException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
