using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using System.Collections.Generic;
using System.Linq;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(ISynchronizersRepository), Lifetime = LifetimeManagement.Scoped)]
    public class WalletSynchronizersRepository : ISynchronizersRepository
    {
        private readonly IEnumerable<ISynchronizer> _walletSynchronizers;

        public WalletSynchronizersRepository(IEnumerable<ISynchronizer> walletSynchronizers)
        {
            _walletSynchronizers = walletSynchronizers;
        }

        public ISynchronizer GetInstance(string key)
        {
            return _walletSynchronizers.FirstOrDefault(s => s.Name == key);
        }
    }
}
