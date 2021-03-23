using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers;

namespace O10.Node.DataLayer.DataServices
{
    [ExtensionPoint]
    public interface IChainDataService : IDataService<IPacketBase>
    {
        LedgerType LedgerType { get; }

        long GetScalar(IDataKey dataKey);
    }
}
