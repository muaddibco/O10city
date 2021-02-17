using System;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Properties;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class BlockTypeNotSupportedException : Exception
    {
        public BlockTypeNotSupportedException() { }
        public BlockTypeNotSupportedException(ushort blockType, LedgerType chainType) : base(string.Format(Resources.ERR_NOT_SUPPORTED_BLOCK_TYPE, blockType, chainType)) { }
        public BlockTypeNotSupportedException(ushort blockType, LedgerType chainType, Exception inner) : base(string.Format(Resources.ERR_NOT_SUPPORTED_BLOCK_TYPE, blockType, chainType), inner) { }
        protected BlockTypeNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
