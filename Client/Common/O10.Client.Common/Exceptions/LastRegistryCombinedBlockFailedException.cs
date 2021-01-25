using System;
using O10.Client.Common.Properties;

namespace O10.Client.Common.Exceptions
{

    [Serializable]
    public class LastRegistryCombinedBlockFailedException : Exception
    {
        public LastRegistryCombinedBlockFailedException() : base(Resources.ERR_LAST_REGISTRY_COMBINED_BLOCK_FAILED) { }
        public LastRegistryCombinedBlockFailedException(Exception inner) : base(Resources.ERR_LAST_REGISTRY_COMBINED_BLOCK_FAILED, inner) { }
        protected LastRegistryCombinedBlockFailedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
