using System;
using O10.Client.Web.Portal.Properties;

namespace O10.Client.Web.Portal.Exceptions
{

    [Serializable]
    public class RootAttributeTransferFailedException : Exception
    {
        public RootAttributeTransferFailedException() : base(Resources.ERR_FAILED_TRANSFER_ROOT_ATTR) { }
        public RootAttributeTransferFailedException(Exception inner) : base(Resources.ERR_FAILED_TRANSFER_ROOT_ATTR, inner) { }
        protected RootAttributeTransferFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
