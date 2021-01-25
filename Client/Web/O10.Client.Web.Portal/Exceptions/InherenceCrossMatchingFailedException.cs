using System;
using O10.Client.Web.Portal.Properties;

namespace O10.Client.Web.Portal.Exceptions
{

    [Serializable]
    public class InherenceCrossMatchingFailedException : Exception
    {
        public InherenceCrossMatchingFailedException() { }
        public InherenceCrossMatchingFailedException(string registrationKey) : base(string.Format(Resources.ERR_INHERENCE_MATCHING_FAILED, registrationKey)) { }
        public InherenceCrossMatchingFailedException(string registrationKey, Exception inner) : base(string.Format(Resources.ERR_INHERENCE_MATCHING_FAILED, registrationKey), inner) { }
        protected InherenceCrossMatchingFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
