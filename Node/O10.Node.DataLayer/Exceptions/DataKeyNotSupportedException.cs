using System;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Node.DataLayer.Properties;

namespace O10.Node.DataLayer.Exceptions
{

    [Serializable]
    public class DataKeyNotSupportedException : Exception
    {
        public DataKeyNotSupportedException() { }
        public DataKeyNotSupportedException(IDataKey key) : base(string.Format(Resources.ERR_NOT_IMPLEMENTED_FOR_KEY, key.GetType().FullName)) { }
        public DataKeyNotSupportedException(IDataKey key, Exception inner) : base(string.Format(Resources.ERR_NOT_IMPLEMENTED_FOR_KEY, key.GetType().FullName), inner) { }
        protected DataKeyNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
