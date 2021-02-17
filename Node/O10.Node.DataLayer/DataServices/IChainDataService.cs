using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Node.DataLayer.DataServices
{
    [ExtensionPoint]
    public interface IChainDataService : IDataService<PacketBase>
    {
        LedgerType PacketType { get; }

        IChainDataServicesManager ChainDataServicesManager { set; }

        ulong GetScalar(IDataKey dataKey);
    }
}
