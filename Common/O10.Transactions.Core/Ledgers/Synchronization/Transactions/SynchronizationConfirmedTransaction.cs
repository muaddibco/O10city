using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Synchronization.Transactions
{
    public class SynchronizationConfirmedTransaction : SynchronizationTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Synchronization_ConfirmedBlock;
    
        public ushort Round { get; set; }

        public byte[][] Signatures { get; set; }

        public byte[][] PublicKeys { get; set; }
    }
}
