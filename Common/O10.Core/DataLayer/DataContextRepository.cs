using System.Collections.Generic;
using System.Linq;
using O10.Core.Architecture;


namespace O10.Core.DataLayer
{
    [RegisterDefaultImplementation(typeof(IDataContextRepository), Lifetime = LifetimeManagement.Singleton)]
    public class DataContextRepository : IDataContextRepository
    {
        private readonly IEnumerable<IDataContext> _dataContexts;

        public DataContextRepository(IEnumerable<IDataContext> dataContexts)
        {
            _dataContexts = dataContexts;
        }

        public T GetInstance<T>(string key) where T : IDataContext
        {
            T dc = (T)_dataContexts.FirstOrDefault(s => s is T && s.DataProvider == key);

            return dc;
        }
    }
}
