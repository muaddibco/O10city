using O10.Core;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;

namespace O10.Gateway.Common.Services.LedgerSynchronizers
{
    [ServiceContract]
    public interface ILedgerSynchronizersRepository : IRepository<ILedgerSynchronizer, LedgerType>
    {
    }
}
