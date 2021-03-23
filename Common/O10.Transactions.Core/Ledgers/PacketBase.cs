using O10.Core.Models;
using O10.Crypto.Models;
using O10.Transactions.Core.Enums;

namespace O10.Transactions.Core.Ledgers
{
    /// <summary>
    /// All packetss in all types of ledgers must inherit from this base class
    /// </summary>
    public abstract class PacketBase<TTransaction, TSignature> : SerializableEntity<PacketBase<TTransaction, TSignature>>, IPacketBase where TTransaction : TransactionBase where TSignature : SignatureBase //SerializableEntity<PacketBase<TTransaction, TSignature>>, ILedgerPacket where TTransaction : TransactionBase where TSignature : SignatureBase
    {
        public TTransaction? Body { get; set; }

        public T? With<T>() where T : TTransaction
        {
            return Body as T;
        }

        public T? AsPacket<T>() where T : class, IPacketBase
        {
            return this as T;
        }

        public TSignature? Signature { get; set; }

        public abstract LedgerType LedgerType { get; }

        TransactionBase? IPacketBase.Body { get => Body; }

        SignatureBase? IPacketBase.Signature { get => Signature; }
    }

    public interface IPacketBase: ISerializableEntity<IPacketBase>
    {
        LedgerType LedgerType { get; }

        TransactionBase? Body { get; }

        SignatureBase? Signature { get; }

        T? AsPacket<T>() where T: class, IPacketBase;
    }
}
