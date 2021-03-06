﻿using System;
using O10.Core.HashCalculations;
using O10.Core.Properties;

namespace O10.Core.Exceptions
{

    [Serializable]
    public class HashAlgorithmNotSupportedException : Exception
    {
        public HashAlgorithmNotSupportedException() { }
        public HashAlgorithmNotSupportedException(HashType hashType) : base(string.Format(Resources.ERR_HASH_ALGORITHM_NOT_SUPPORTED, hashType)) { }
        public HashAlgorithmNotSupportedException(HashType hashType, Exception inner) : base(string.Format(Resources.ERR_HASH_ALGORITHM_NOT_SUPPORTED, hashType), inner) { }
        protected HashAlgorithmNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
