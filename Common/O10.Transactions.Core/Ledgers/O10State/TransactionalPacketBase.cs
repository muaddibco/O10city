using O10.Core.Models;

namespace O10.Transactions.Core.Ledgers.O10State
{
    public abstract class TransactionalPacketBase : SignedPacketBase
    {
        public override ushort LedgerType => (ushort)Enums.LedgerType.O10State;

        /// <summary>
        /// Up to date funds at last transactional block
        /// </summary>
        public ulong UptodateFunds { get; set; }
    }
}
