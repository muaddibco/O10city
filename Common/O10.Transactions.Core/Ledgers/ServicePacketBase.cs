namespace O10.Transactions.Core.Ledgers
{
    public abstract class ServicePacketBase : OrderedTransactionBase
    {
        public ulong SyncHeight { get; set; }
    }
}
