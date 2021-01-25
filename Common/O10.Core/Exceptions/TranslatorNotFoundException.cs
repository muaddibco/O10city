﻿using System;
using O10.Core.Properties;

namespace O10.Core.Exceptions
{

    [Serializable]
    public class TranslatorNotFoundException : Exception
    {
        public TranslatorNotFoundException() { }
        public TranslatorNotFoundException(string from, string to) : base(string.Format(Resources.ERR_TRANSLATOR_NOT_FOUND, from, to)) { }
        public TranslatorNotFoundException(string from, string to, Exception inner) : base(string.Format(Resources.ERR_TRANSLATOR_NOT_FOUND, from, to), inner) { }
        protected TranslatorNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
