using System.Net;
using O10.Core.Identity;
using O10.Core.Models;

namespace O10.Network.Topology
{
    public class Node : SerializableEntity
    {
        public IKey Key { get; set; }

        public IPAddress IPAddress { get; set; }

        public NodeRole NodeRole { get; set; }
    }
}
