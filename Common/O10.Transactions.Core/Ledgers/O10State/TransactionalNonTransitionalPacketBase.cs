namespace O10.Transactions.Core.Ledgers.O10State
{
    public abstract class TransactionalNonTransitionalPacketBase : TransactionalPacketBase
	{
		public byte[] Target { get; set; }
	}
}
