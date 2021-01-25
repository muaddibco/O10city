using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using System.Collections.Generic;
using System.Linq;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IWalletSynchronizersRepository), Lifetime = LifetimeManagement.Scoped)]
    public class WalletSynchronizersRepository : IWalletSynchronizersRepository
    {
        private readonly IEnumerable<IWalletSynchronizer> _walletSynchronizers;

        public WalletSynchronizersRepository(IEnumerable<IWalletSynchronizer> walletSynchronizers)
        {
            _walletSynchronizers = walletSynchronizers;
        }

        public IWalletSynchronizer GetInstance(string key)
        {
            return _walletSynchronizers.FirstOrDefault(s => s.Name == key);
        }
    }
}
