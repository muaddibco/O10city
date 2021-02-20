using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DTOs
{
    public class TransactionInfo
    {
        public ulong SyncBlockHeight { get; set; }
        public LedgerType LedgerType { get; set; }
        public ushort PacketType { get; set; }
        public byte[] Content { get; set; }
    }
}
