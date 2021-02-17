using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Transactions.Core.Interfaces
{
    [ServiceContract]
    public interface IBlocksHandlersRegistry : IRepository<IBlocksHandler, string>, IBulkRegistry<IBlocksHandler, LedgerType>
    {
    }
}
