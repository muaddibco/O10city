using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;

namespace O10.Node.DataLayer.DataServices
{
    [RegisterDefaultImplementation(typeof(IChainDataServicesRepository), Lifetime = LifetimeManagement.Scoped)]
    public class ChainDataServicesRepository : IChainDataServicesRepository
    {
        private readonly IEnumerable<IChainDataService> _chainDataServices;
        public ChainDataServicesRepository(IEnumerable<IChainDataService> chainDataServices)
        {
            _chainDataServices = chainDataServices;
        }

		public IEnumerable<IChainDataService> GetInstances() => _chainDataServices;

		public IChainDataService GetInstance(LedgerType chainType)
        {
            return _chainDataServices.FirstOrDefault(c => c.LedgerType == chainType);
        }
    }
}
