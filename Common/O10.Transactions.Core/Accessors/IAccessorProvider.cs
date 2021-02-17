using O10.Core;
using O10.Core.Architecture;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Accessors
{
    [ServiceContract]
    public interface IAccessorProvider : IRepository<IAccessor, LedgerType>
    {
    }
}
