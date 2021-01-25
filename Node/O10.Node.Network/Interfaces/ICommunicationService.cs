using System.Collections.Generic;
using O10.Network.Communication;
using O10.Core.Architecture;
using O10.Core.Communication;
using O10.Core.Identity;

namespace O10.Network.Interfaces
{
    [ExtensionPoint]
    public interface ICommunicationService
    {
        string Name { get; }

        void Stop();

        void Start();
        void Init(SocketSettings settings);

        void PostMessage(IKey destination, IPacketProvider message);

        void PostMessage(IEnumerable<IKey> destinations, IPacketProvider message);
    }
}
