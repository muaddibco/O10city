using System.Threading;
using O10.Transactions.Core.Enums;
using O10.Core.Architecture;
using O10.Core.Models;

namespace O10.Transactions.Core.Interfaces
{
    [ExtensionPoint]
    public interface IBlocksHandler
    {
        string Name { get; }

        LedgerType LedgerType { get; }

        void Initialize(CancellationToken ct);
        void ProcessBlock(PacketBase blockBase);
    }
}
