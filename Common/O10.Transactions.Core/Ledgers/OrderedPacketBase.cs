using O10.Crypto.Models;

namespace O10.Transactions.Core.Ledgers
{
    public abstract class OrderedPacketBase<TTransaction, TSignature> : PacketBase<TTransaction, TSignature> where TTransaction : TransactionBase where TSignature : SignatureBase
    {
        public long Height { get; set; }

    }
}
