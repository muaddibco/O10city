using O10.Core.Identity;
using O10.Transactions.Core.Properties;
using System;

namespace O10.Transactions.Core.Exceptions
{

    [Serializable]
    public class WitnessedKeyImageException : Exception
    {
        public WitnessedKeyImageException() { }
        public WitnessedKeyImageException(IKey keyImage) : base(string.Format(Resources.ERR_DUPLICATED_KEY_IMAGE, keyImage)) { }
        public WitnessedKeyImageException(IKey keyImage, Exception inner) : base(string.Format(Resources.ERR_DUPLICATED_KEY_IMAGE, keyImage), inner) { }
        protected WitnessedKeyImageException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
