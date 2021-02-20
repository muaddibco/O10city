using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Node.DataLayer.DataAccess
{
    [ExtensionPoint]
    public interface INodeDataContext : IDataContext
    {
        LedgerType LedgerType { get; }
    }
}
