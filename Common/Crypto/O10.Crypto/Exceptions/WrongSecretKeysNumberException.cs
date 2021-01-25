using System;
using O10.Crypto.Properties;

namespace O10.Crypto.Exceptions
{

    [Serializable]
    public class WrongSecretKeysNumberException : Exception
    {
        public WrongSecretKeysNumberException() { }
        public WrongSecretKeysNumberException(string serviceName, int expectedSecretKeysNumber) : base(string.Format(Resources.ERR_WRONG_SECRECT_KEYS_NUMBER, serviceName, expectedSecretKeysNumber)) { }
        public WrongSecretKeysNumberException(string serviceName, int expectedSecretKeysNumber, Exception inner) : base(string.Format(Resources.ERR_WRONG_SECRECT_KEYS_NUMBER, serviceName, expectedSecretKeysNumber), inner) { }
        protected WrongSecretKeysNumberException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
