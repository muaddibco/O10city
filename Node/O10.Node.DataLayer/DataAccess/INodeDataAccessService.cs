using O10.Transactions.Core.Enums;
using O10.Core.Persistency;

namespace O10.Node.DataLayer.DataAccess
{
    public interface INodeDataAccessService : IDataAccessService
    {
        LedgerType LedgerType { get; }
    }
}
