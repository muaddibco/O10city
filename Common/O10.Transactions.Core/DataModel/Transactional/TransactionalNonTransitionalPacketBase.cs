namespace O10.Transactions.Core.DataModel.Transactional
{
    public abstract class TransactionalNonTransitionalPacketBase : TransactionalPacketBase
	{
		public byte[] Target { get; set; }
	}
}
