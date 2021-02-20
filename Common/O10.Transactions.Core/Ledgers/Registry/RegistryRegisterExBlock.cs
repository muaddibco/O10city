using O10.Transactions.Core.Enums;
using System.Collections.Generic;

namespace O10.Transactions.Core.Ledgers.Registry
{
    public class RegistryRegisterExBlock : RegistryBlockBase
    {
        public override ushort PacketType => PacketTypes.Registry_RegisterEx;

        public override ushort Version => 1;

        public LedgerType ReferencedLedgerType { get; set; }

        public ushort ReferencedAction { get; set; }

        public Dictionary<string, string> Parameters { get; set; }
    }
}
