using O10.Transactions.Core.Enums;
using System.Collections.Generic;

namespace O10.Transactions.Core.Accessors
{
    public class EvidenceDescriptor
    {
        public PacketType PacketType { get; set; }
        public ushort ActionType { get; set; }

        public Dictionary<string, string> Parameters { get; set; }
    }
}
