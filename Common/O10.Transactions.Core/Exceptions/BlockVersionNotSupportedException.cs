using System;
using O10.Transactions.Core.Properties;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class BlockVersionNotSupportedException : Exception
    {
        public BlockVersionNotSupportedException() { }
        public BlockVersionNotSupportedException(ushort version, ushort blockType) : base(string.Format(Resources.ERR_BLOCK_VERSION_NOT_SUPPORTED, version, blockType)) { }
        public BlockVersionNotSupportedException(string version, ushort blockType, Exception inner) : base(string.Format(Resources.ERR_BLOCK_VERSION_NOT_SUPPORTED, version, blockType), inner) { }
        protected BlockVersionNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
