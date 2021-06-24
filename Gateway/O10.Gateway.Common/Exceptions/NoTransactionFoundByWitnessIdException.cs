using O10.Gateway.Common.Properties;
using System;

namespace O10.Gateway.Common.Exceptions
{

    [Serializable]
    public class NoTransactionFoundByWitnessIdException : Exception
    {
        public NoTransactionFoundByWitnessIdException() { }
        public NoTransactionFoundByWitnessIdException(long witnessId) : base(string.Format(Resources.ERR_NO_TRANSACTION_FOUND, witnessId)) { }
        public NoTransactionFoundByWitnessIdException(long witnessId, Exception inner) : base(string.Format(Resources.ERR_NO_TRANSACTION_FOUND, witnessId), inner) { }
        protected NoTransactionFoundByWitnessIdException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
