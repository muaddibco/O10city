using O10.Core.Architecture;
using O10.Core.Models;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Accessors
{
    /// <summary>
    /// Accessor is a service class that accesses relevant chains and obtains packets using description
    /// </summary>
    [ExtensionPoint]
    public interface IAccessor
    {
        PacketType PacketType { get; }

        T GetPacket<T>(EvidenceDescriptor accessDescriptor) where T : PacketBase;
    }
}
