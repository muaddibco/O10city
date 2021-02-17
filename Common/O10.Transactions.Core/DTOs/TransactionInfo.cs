using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DTOs
{
    public class TransactionInfo
    {
        public ulong SyncBlockHeight { get; set; }
        public LedgerType PacketType { get; set; }
        public ushort BlockType { get; set; }
        public byte[] Content { get; set; }
    }
}
