using O10.Core.Architecture;

namespace O10.Core.Persistency
{
    [ServiceContract]
    public interface IDataAccessServiceRepository : IRepository<IDataAccessService>
    {
    }
}
