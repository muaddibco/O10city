using O10.Core.Architecture;
using System.Collections.Generic;
using System.Linq;

namespace O10.Core.Persistency
{
    [RegisterDefaultImplementation(typeof(IDbConnectionProvidersRepository), Lifetime = LifetimeManagement.Singleton)]
    public class DbConnectionProvidersRepository : IDbConnectionProvidersRepository
    {
        private readonly IEnumerable<IDbConnectionProvider> _providers;

        public DbConnectionProvidersRepository(IEnumerable<IDbConnectionProvider> providers)
        {
            _providers = providers;
        }

        public IDbConnectionProvider GetInstance(string key)
        {
            return _providers.FirstOrDefault(s => s.ConnectionType == key);
        }
    }
}
