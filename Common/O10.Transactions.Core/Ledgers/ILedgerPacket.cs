using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers
{
    public interface ILedgerPacket
    {
        LedgerType LedgerType { get; }
    }
}
