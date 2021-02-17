using O10.Transactions.Core.Enums;
using O10.Core.DataLayer;

namespace O10.Node.DataLayer.DataAccess
{
    public interface INodeDataAccessService : IDataAccessService
    {
        LedgerType PacketType { get; }
    }
}
