using System;
using O10.Client.Web.Portal.Properties;

namespace O10.Client.Web.Portal.Exceptions
{

    [Serializable]
    public class InherenceRegistrationProofsIncorrectException : Exception
    {
        public InherenceRegistrationProofsIncorrectException() : base(Resources.ERR_INHERENCE_REGISTRATION_PROOFS_INCORRECT) { }
        public InherenceRegistrationProofsIncorrectException(Exception inner) : base(Resources.ERR_INHERENCE_REGISTRATION_PROOFS_INCORRECT, inner) { }
        protected InherenceRegistrationProofsIncorrectException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
