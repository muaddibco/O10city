using O10.Transactions.Core.Enums;
using System;

namespace O10.Transactions.Core.Ledgers.Registry.Transactions
{
    public class FullRegistryTransaction : RegistryTransactionBase
    {
        public override ushort TransactionType => TransactionTypes.Registry_FullBlock;

        public RegistryPacket[] Witnesses { get; set; }

        [Obsolete("Need to remove from use")]
        public byte[] ShortBlockHash { get; set; }
    }
}
