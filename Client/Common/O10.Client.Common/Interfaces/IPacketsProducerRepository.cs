using O10.Core;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;

namespace O10.Client.Common.Interfaces
{
    [ServiceContract]
    public interface IPacketsProducerRepository : IRepository<IPacketsProducer, LedgerType>
    {
    }
}
