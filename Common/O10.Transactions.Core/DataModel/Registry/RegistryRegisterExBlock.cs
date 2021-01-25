using O10.Transactions.Core.Enums;
using System.Collections.Generic;

namespace O10.Transactions.Core.DataModel.Registry
{
    public class RegistryRegisterExBlock : RegistryBlockBase
    {
        public override ushort BlockType => ActionTypes.Registry_RegisterEx;

        public override ushort Version => 1;

        public PacketType ReferencedPacketType { get; set; }

        public ushort ReferencedAction { get; set; }

        public Dictionary<string, string> Parameters { get; set; }
    }
}
