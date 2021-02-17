using System.Collections.Generic;
using System.Linq;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;

namespace O10.Node.DataLayer.DataServices
{
    [RegisterDefaultImplementation(typeof(IChainDataServicesManager), Lifetime = LifetimeManagement.Singleton)]
    public class ChainDataServicesManager : IChainDataServicesManager
    {
        private readonly IEnumerable<IChainDataService> _chainDataServices;
        public ChainDataServicesManager(IEnumerable<IChainDataService> chainDataServices)
        {
            _chainDataServices = chainDataServices;
        }

		public IEnumerable<IChainDataService> GetAll() => _chainDataServices;

		public IChainDataService GetChainDataService(LedgerType chainType)
        {
            return _chainDataServices.FirstOrDefault(c => c.PacketType == chainType);
        }
    }
}
