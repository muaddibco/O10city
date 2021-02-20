using O10.Transactions.Core.Actions;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public class UniversalStatePacket : TransactionalPacketBase
    {
        public override ushort Version => 1;

        public override ushort PacketType => PacketTypes.Transaction_UniversalTransport;

        public ActionBase? Action { get; set; }
    }
}
