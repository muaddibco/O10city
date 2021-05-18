using O10.Client.Common.Interfaces;
using O10.Core;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;

namespace O10.Client.Web.Common.Services
{
    [ServiceContract]
    public interface ILedgerWriterRepository : IRepository<ILedgerWriter, LedgerType>
    {
    }
}
