using System.Collections.Generic;
using System.Linq;
using O10.Core.Architecture;


namespace O10.Core.Persistency
{
    [RegisterDefaultImplementation(typeof(IDataAccessServiceRepository), Lifetime = LifetimeManagement.Singleton)]
    public class DataAccessServiceRepository : IDataAccessServiceRepository
    {
        private readonly IEnumerable<IDataAccessService> _dataAccessServices;

        public DataAccessServiceRepository(IEnumerable<IDataAccessService> dataAccessServices)
        {
            _dataAccessServices = dataAccessServices;
        }

        public IDataAccessService GetInstance()
        {
            return _dataAccessServices.FirstOrDefault(s => s is IDataAccessService);
        }

        public T1 GetInstance<T1>() where T1 : IDataAccessService
        {
            return (T1)_dataAccessServices.FirstOrDefault(s => s is T1);
        }
    }
}
