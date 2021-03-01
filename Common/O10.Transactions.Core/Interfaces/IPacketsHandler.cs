using System.Threading;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers;

namespace O10.Transactions.Core.Interfaces
{
    [ExtensionPoint]
    public interface IPacketsHandler
    {
        string Name { get; }

        LedgerType LedgerType { get; }

        void Initialize(CancellationToken ct);
        void ProcessBlock(IPacketBase blockBase);
    }
}
