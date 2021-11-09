using System.Net;
using O10.Core.Identity;

namespace O10.Node.Network.Topology
{
	public class NodeAddress
    {
        public NodeAddress(IKey key, IPAddress ipAddress)
        {
            Key = key;
            IP = ipAddress;
        }

        public IKey Key { get; set; }

        public IPAddress IP { get; set; }
    }
}
