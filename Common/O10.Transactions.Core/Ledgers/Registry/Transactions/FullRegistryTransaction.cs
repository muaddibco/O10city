using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers.Registry.Transactions
{
    public class FullRegistryTransaction : RegistryTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Registry_FullBlock;
        public RegistryPacket[] Witnesses { get; set; }
        public byte[] ShortBlockHash { get; set; }
    }
}
