using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Transactions.Core.Interfaces
{
    [ServiceContract]
    public interface IPacketsHandlersRegistry : IRepository<IPacketsHandler, string>, IBulkRegistry<IPacketsHandler, LedgerType>
    {
    }
}
