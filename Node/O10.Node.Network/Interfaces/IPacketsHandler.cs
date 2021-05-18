using System.Threading;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers;

namespace O10.Network.Interfaces
{
    [ExtensionPoint]
    public interface ILedgerPacketsHandler
    {
        string Name { get; }

        LedgerType LedgerType { get; }

        void Initialize(CancellationToken ct);

        void ProcessPacket(IPacketBase blockBase);
    }
}
