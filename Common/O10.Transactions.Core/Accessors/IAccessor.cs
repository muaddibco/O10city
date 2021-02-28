using O10.Core.Architecture;
using O10.Transactions.Core.Enums;
using O10.Transactions.Core.Ledgers;
using System.Threading.Tasks;

namespace O10.Transactions.Core.Accessors
{
    /// <summary>
    /// Accessor is a service class that accesses relevant chains and obtains packets using description
    /// </summary>
    [ExtensionPoint]
    public interface IAccessor
    {
        LedgerType LedgerType { get; }

        Task<T> GetPacket<T>(EvidenceDescriptor accessDescriptor) where T : PacketBase;
    }
}
