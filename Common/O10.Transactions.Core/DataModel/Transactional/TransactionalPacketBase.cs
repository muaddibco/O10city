using O10.Core.Models;

namespace O10.Transactions.Core.DataModel.Transactional
{
    public abstract class TransactionalPacketBase : SignedPacketBase
    {
        public override ushort PacketType => (ushort)Enums.PacketType.Transactional;

        /// <summary>
        /// Up to date funds at last transactional block
        /// </summary>
        public ulong UptodateFunds { get; set; }
    }
}
