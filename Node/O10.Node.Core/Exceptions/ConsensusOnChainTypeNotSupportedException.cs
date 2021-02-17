using System;
using O10.Transactions.Core.Enums;
using O10.Node.Core.Properties;

namespace O10.Node.Core.Exceptions
{

    [Serializable]
    public class ConsensusOnChainTypeNotSupportedException : Exception
    {
        public ConsensusOnChainTypeNotSupportedException() { }
        public ConsensusOnChainTypeNotSupportedException(LedgerType chainType) : base(string.Format(Resources.ERR_CONSENSUS_ON_CHAINTYPE_NOT_SUPPORTED, chainType)) { }
        public ConsensusOnChainTypeNotSupportedException(LedgerType chainType, Exception inner) : base(string.Format(Resources.ERR_CONSENSUS_ON_CHAINTYPE_NOT_SUPPORTED, chainType), inner) { }
        protected ConsensusOnChainTypeNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
