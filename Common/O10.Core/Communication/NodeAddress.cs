using System.Net;
using O10.Core.Identity;

namespace O10.Core.Communication
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
