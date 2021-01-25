using System;
using System.Runtime.Serialization;
using O10.Core.Properties;

namespace O10.Core.Exceptions
{
    [Serializable]
    public class FailedToFindLogConfigFileException : Exception
    {
        public FailedToFindLogConfigFileException() { }
        public FailedToFindLogConfigFileException(string logComponent, string path) : base(string.Format(Resources.ERR_FAILED_TO_FIND_LOG_CONFIG_FILE, logComponent, path)) { }
        public FailedToFindLogConfigFileException(string logComponent, string path, Exception inner) : base(string.Format(Resources.ERR_FAILED_TO_FIND_LOG_CONFIG_FILE, logComponent, path), inner) { }
        protected FailedToFindLogConfigFileException(
          SerializationInfo info,
          StreamingContext context) : base(info, context)
        { }
    }
}
