using O10.Transactions.Core.Enums;
using O10.Core;
using O10.Core.Architecture;

namespace O10.Network.Interfaces
{
    [ServiceContract]
    public interface IPacketsHandlersRegistry : IRepository<ILedgerPacketsHandler, string>, IBulkRegistry<ILedgerPacketsHandler, LedgerType>
    {
    }
}
