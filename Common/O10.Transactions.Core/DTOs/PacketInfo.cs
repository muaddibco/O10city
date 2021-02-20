using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.DTOs
{
    public class PacketInfo
    {
        public LedgerType LedgerType { get; set; }

        public ushort BlockType { get; set; }

        public byte[] Content { get; set; }
    }
}
