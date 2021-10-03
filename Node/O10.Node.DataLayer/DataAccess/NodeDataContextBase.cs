using O10.Transactions.Core.Enums;
using O10.Core.Persistency;

namespace O10.Node.DataLayer.DataAccess
{
    public abstract class NodeDataContextBase : DataContextBase, INodeDataContext
    {
        public abstract LedgerType LedgerType { get; }
    }
}
