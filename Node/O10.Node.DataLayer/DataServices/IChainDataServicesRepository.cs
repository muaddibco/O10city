using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core;

namespace O10.Node.DataLayer.DataServices
{
	[ServiceContract]
    public interface IChainDataServicesRepository : IBulkRepository<IChainDataService>, IRepository<IChainDataService, LedgerType>
    {
        IChainDataService GetInstance(LedgerType ledgerType);

		IEnumerable<IChainDataService> GetInstances();
    }
}
