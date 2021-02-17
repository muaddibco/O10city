using O10.Core.Models;

namespace O10.Transactions.Core.Ledgers.Synchronization
{
    public abstract class LinkedPacketBase : SignedPacketBase
    {
        /// <summary>
        /// 64 byte value of hash of previous block content
        /// </summary>
        public byte[] HashPrev { get; set; }
    }
}
