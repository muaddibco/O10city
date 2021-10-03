using O10.Core.Architecture;
using System.Data;

namespace O10.Core.Persistency
{
    [ServiceContract]
    public interface IDbConnectionProvidersRepository : IRepository<IDbConnectionProvider, string>
    {
    }
}
