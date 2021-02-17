using O10.Transactions.Core.Enums;
using O10.Core.DataLayer;

namespace O10.Node.DataLayer.DataAccess
{
    public abstract class NodeDataContextBase : DataContextBase, INodeDataContext
    {
        public abstract LedgerType PacketType { get; }
    }
}
