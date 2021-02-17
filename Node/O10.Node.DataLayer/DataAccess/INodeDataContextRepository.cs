using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Node.DataLayer.DataAccess
{
    [ServiceContract]
    public interface INodeDataContextRepository : IRepository<INodeDataContext, LedgerType, string>
    {
    }
}
