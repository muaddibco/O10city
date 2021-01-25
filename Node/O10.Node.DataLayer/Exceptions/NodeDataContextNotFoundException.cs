using System;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.Properties;

namespace O10.Node.DataLayer.Exceptions
{

    [Serializable]
    public class NodeDataContextNotFoundException : Exception
    {
        public NodeDataContextNotFoundException() { }
        public NodeDataContextNotFoundException(PacketType packetType, string dataProvider) : base(string.Format(Resources.ERR_NODE_DATA_CONTEXT_NOT_FOUND, packetType, dataProvider)) { }
        public NodeDataContextNotFoundException(PacketType packetType, string dataProvider, Exception inner) : base(string.Format(Resources.ERR_NODE_DATA_CONTEXT_NOT_FOUND, packetType, dataProvider), inner) { }
        protected NodeDataContextNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
