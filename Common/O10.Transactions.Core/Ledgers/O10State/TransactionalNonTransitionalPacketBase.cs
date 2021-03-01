namespace O10.Transactions.Core.Ledgers.O10State
{
    public abstract class TransactionalNonTransitionalPacketBase : O10StatePacket
	{
		public byte[] Target { get; set; }
	}
}
