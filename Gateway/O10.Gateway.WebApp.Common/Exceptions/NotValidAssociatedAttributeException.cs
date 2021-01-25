using System;
using O10.Gateway.WebApp.Common.Properties;

namespace O10.Gateway.WebApp.Common.Exceptions
{

    [Serializable]
    public class NotValidAssociatedAttributeException : Exception
    {
        public NotValidAssociatedAttributeException() { }
        public NotValidAssociatedAttributeException(string issuanceCommitment, string commitmentToRoot, string issuer) : base(string.Format(Resources.ERR_NOT_VALID_ASSOCIATED_ATTR, issuanceCommitment, commitmentToRoot, issuer)) { }
        public NotValidAssociatedAttributeException(string issuanceCommitment, string commitmentToRoot, string issuer, Exception inner) : base(string.Format(Resources.ERR_NOT_VALID_ASSOCIATED_ATTR, issuanceCommitment, commitmentToRoot, issuer), inner) { }
        protected NotValidAssociatedAttributeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
