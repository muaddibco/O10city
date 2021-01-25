using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class CommitmentNotEligibleException : Exception
    {
        public CommitmentNotEligibleException() : base(Resources.ERR_COMMITMENT_ELIGIBILITY_VERIFICATION_FAILED) { }
        public CommitmentNotEligibleException(Exception inner) : base(Resources.ERR_COMMITMENT_ELIGIBILITY_VERIFICATION_FAILED, inner) { }
        protected CommitmentNotEligibleException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
