using System.Collections.Generic;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;

namespace O10.Node.DataLayer.DataServices
{
	[ServiceContract]
    public interface IChainDataServicesManager
    {
        IChainDataService GetChainDataService(PacketType chainType);

		IEnumerable<IChainDataService> GetAll();
    }
}
