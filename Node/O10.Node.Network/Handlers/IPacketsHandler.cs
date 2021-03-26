using System.Threading;
using O10.Core.Architecture;
using O10.Transactions.Core.Ledgers;

namespace O10.Network.Handlers
{
    /// <summary>
    /// Service receives raw arrays of bytes representing all types of messages exchanges over network. 
    /// Byte arrays must contain exact bytes of message to be processed correctly.
    /// </summary>
    [ServiceContract]
    public interface IPacketsHandler
    {
        void Initialize(CancellationToken ct);

        void Push(IPacketBase packet);

        void Start();
    }
}
