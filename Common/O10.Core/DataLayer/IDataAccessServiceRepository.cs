using O10.Core.Architecture;

namespace O10.Core.DataLayer
{
    [ServiceContract]
    public interface IDataAccessServiceRepository : IRepository<IDataAccessService>
    {
    }
}
