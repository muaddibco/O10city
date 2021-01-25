using O10.Client.Web.Portal.Properties;
using System;

namespace O10.Client.Web.Portal.Exceptions
{

    [Serializable]
    public class MandatoryAttributeValueMissingException : Exception
    {
        public MandatoryAttributeValueMissingException() { }
        public MandatoryAttributeValueMissingException(string attributeName) : base(string.Format(Resources.ERR_MANDATORY_ATTRIBUTE_NOT_PROVIDED, attributeName)) { }
        public MandatoryAttributeValueMissingException(string attributeName, Exception inner) : base(string.Format(Resources.ERR_MANDATORY_ATTRIBUTE_NOT_PROVIDED, attributeName), inner) { }
        protected MandatoryAttributeValueMissingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
